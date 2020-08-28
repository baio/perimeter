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
import { AuthService } from '@perimeter/ngx-auth';
import { Observable } from 'rxjs';
import { map, withLatestFrom } from 'rxjs/operators';

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
        const jwt = this.authService.validateTokens(true);
        if (!jwt) {
            // If no auth token found redirect to login page
            return false;
        }
        return this.store.select(selectStatus).pipe(
            filterCompletedStatuses$,
            withLatestFrom(this.store.select(selectActiveDomain(state.url))),
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
    }
}
