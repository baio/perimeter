import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { EMPTY, of } from 'rxjs';
import {
    map,
    mergeMap,
    catchError,
    switchMap,
    tap,
    withLatestFrom,
} from 'rxjs/operators';
import { AuthService } from '@perimeter/ngx-auth';
import {
    authenticate,
    authenticationSuccess,
    authenticationFails,
    profileLoadSuccess,
    profileLoadFails,
} from './actions';
import { ProfileDataAccessService } from '@admin/data-access';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { selectProfileDomainsList } from './selectors';
import { HttpErrorResponse } from '@angular/common/http';

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

    initFails$ = createEffect(
        () =>
            this.actions$.pipe(
                ofType(authenticationFails, profileLoadFails),
                tap(() => {
                    this.router.navigate(['/home']);
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
                                tenantManagementDomain.id,
                                'domains',
                            ]);
                        } else {
                            // TODO : If no domains
                            this.router.navigate([
                                'domains',
                                domains[0].id,
                                'apps',
                            ]);
                        }
                    }
                })
            ),
        { dispatch: false }
    );
}
