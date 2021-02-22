import { ProfileDataAccessService } from '@admin/data-access';
import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { AuthService } from '@perimeter/ngx-auth';
import { of } from 'rxjs';
import { catchError, delay, map, switchMap, tap } from 'rxjs/operators';
import {
    authenticate,
    authenticationFails,
    authenticationSuccess,
    loadManagementDomains,
    loadManagementDomainsSuccess,
    profileLoadFails,
    profileLoadSuccess,
} from './actions';

@Injectable()
export class ProfileEffects {
    constructor(
        private readonly actions$: Actions,
        private readonly authService: AuthService,
        private readonly profileDataAccess: ProfileDataAccessService,
        private readonly router: Router,
        private readonly store: Store
    ) {}

    authenticate$ = createEffect(() =>
        this.actions$.pipe(
            ofType(authenticate),
            switchMap(async () => {
                // try get id token (access token will be also checked)
                let idToken = this.authService.validateTokens();
                if (!idToken) {
                    // if idToken is not valid try to refresh them
                    const refreshed = await this.authService.refreshToken();
                    if (refreshed) {
                        // if tokens were refreshed try to validate id and access token again
                        idToken = this.authService.validateTokens();
                    } else {
                        this.authService.resetTokens();
                    }
                }
                if (idToken) {
                    // parse user
                    return authenticationSuccess({
                        user: {
                            name:
                                idToken.given_name + ' ' + idToken.family_name,
                        },
                    });
                } else {
                    return authenticationFails();
                }
            })
        )
    );

    authenticationSuccess$ = createEffect(() =>
        this.actions$.pipe(
            ofType(authenticationSuccess),
            switchMap(() => this.profileDataAccess.loadManagementDomains()),
            map((domains) => profileLoadSuccess({ domains })),
            catchError((err: HttpErrorResponse) =>
                of(profileLoadFails({ err }))
            )
        )
    );

    loadManagementDomains$ = createEffect(() =>
        this.actions$.pipe(
            ofType(loadManagementDomains),
            switchMap(() => this.profileDataAccess.loadManagementDomains()),
            map((domains) => loadManagementDomainsSuccess({ domains }))
        )
    );

    initFails$ = createEffect(
        () =>
            this.actions$.pipe(
                ofType(authenticationFails, profileLoadFails),
                tap(() => {
                    this.router.navigate(['/home'], {
                        queryParamsHandling: 'preserve',
                    });
                })
            ),
        { dispatch: false }
    );

    profileLoaded$ = createEffect(
        () =>
            this.actions$.pipe(
                ofType(profileLoadSuccess),
                tap(({ domains }) => {
                    const activeUrl = this.router.routerState.snapshot.url;
                    // when user login and profile loaded, decide here where to redirect
                    // 1. tenant management domains
                    // 2. any other domain
                    if (activeUrl === '/') {
                        const tenantManagementDomain = domains.find(
                            (x) => x.isTenantManagement
                        );
                        if (tenantManagementDomain) {
                            this.router.navigate([
                                'tenants',
                                tenantManagementDomain.tenant.id,
                                'domains',
                            ]);
                        } else if (domains.length > 0) {
                            this.router.navigate([
                                'domains',
                                domains[0].id,
                                'apps',
                            ]);
                        } else {
                            this.router.navigate(['profile', 'home']);
                        }
                    }
                })
            ),
        { dispatch: false }
    );
}
