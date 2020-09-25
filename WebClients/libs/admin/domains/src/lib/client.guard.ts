import {
    filterCompletedStatuses$,
    selectProfileDomainsList,
    selectStatus,
    selectActiveDomain,
    getActiveDomain,
} from '@admin/profile';
import { Injectable } from '@angular/core';
import {
    ActivatedRouteSnapshot,
    CanActivateChild,
    RouterStateSnapshot,
} from '@angular/router';
import { Store } from '@ngrx/store';
import { AuthService, JWTToken } from '@perimeter/ngx-auth';
import { from, Observable, throwError } from 'rxjs';
import { map, switchMap, withLatestFrom } from 'rxjs/operators';

export const ROUTE_NOT_AUTHENTICATED = 'ROUTE_NOT_AUTHENTICATED';

/**
 * Copied over from Angular Router
 * @see https://goo.gl/8qUsNa
 */
export const NAVIGATION_CANCELING_ERROR = 'ngNavigationCancelingError';

/**
 * Similar to navigationCancelingError
 * @see https://goo.gl/nNd9TX
 */
export function makeCancelingError(error: Error) {
    (error as any)[NAVIGATION_CANCELING_ERROR] = true;
    return error;
}

@Injectable()
export class ClientGuard implements CanActivateChild {
    constructor(
        private readonly store: Store,
        private readonly authService: AuthService
    ) {}

    canActivateChild(
        next: ActivatedRouteSnapshot,
        state: RouterStateSnapshot
    ): boolean | Observable<boolean> {
        const validate = (jwt: JWTToken) =>
            this.store.select(selectStatus).pipe(
                filterCompletedStatuses$,
                withLatestFrom(
                    this.store.select(selectActiveDomain(state.url))
                ),
                map(([status, domain]) => {
                    if (status !== 'success') {
                        // profile is not loaded correctly
                        return false;
                    }
                    // find appropriate domain for the current url
                    if (!domain) {
                        // domain not found
                        console.warn('Domain for current url is not found');
                        return false;
                    }

                    const cid = jwt.cid as string;

                    if (domain.managementClientId !== cid) {
                        console.warn(
                            'Current domain clientId is different from authenticated client, relogin under url client'
                        );
                        this.authService.authorize({
                            useSSO: true,
                            clientId: domain.managementClientId,
                            redirectPath: state.url,
                        });
                        return false;
                    } else {
                        return true;
                    }
                })
            );
        return from(this.authService.validateTokensWithRefresh()).pipe(
            switchMap((jwt) => {
                if (!jwt) {
                    // DEBUG
                    debugger;
                }
                return jwt
                    ? validate(jwt)
                    : throwError(
                          makeCancelingError(new Error(ROUTE_NOT_AUTHENTICATED))
                      );
            })
        );
    }
}
