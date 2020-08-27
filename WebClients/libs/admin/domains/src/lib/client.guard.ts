import {
    selectProfileDomainsList,
    selectStatus,
    filterCompletedStatuses$,
} from '@admin/profile';
import { Injectable } from '@angular/core';
import {
    ActivatedRouteSnapshot,
    CanActivateChild,
    RouterStateSnapshot,
} from '@angular/router';
import { Store } from '@ngrx/store';
import { AuthService } from '@perimeter/ngx-auth';
import { includes } from 'lodash/fp';
import { Observable } from 'rxjs';
import { map, skipWhile, withLatestFrom } from 'rxjs/operators';

const getObjIdFromUrl = (url: string) => {
    if (/^\/tenants\/(\d+)/.test(url)) {
        const m = url.match(/^\/tenants\/(\d+)/);
        return { tenant: +m[1] };
    } else if (/^\/domains\/(\d+)/.test(url)) {
        const m = url.match(/^\/domains\/(\d+)/);
        return { domain: +m[1] };
    } else {
        return null;
    }
};

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
        const jwt = this.authService.validateTokens(false);
        if (!jwt) {
            // If no auth token found redirect to login page
            return false;
        }
        return this.store.select(selectStatus).pipe(
            filterCompletedStatuses$,
            withLatestFrom(this.store.select(selectProfileDomainsList)),
            map(([status, domains]) => {
                if (status !== 'success') {
                    // profile is not loaded correctly
                    return false;
                }
                const cid = jwt.cid as string;
                const urlObj = getObjIdFromUrl(state.url);
                // find appropriate domain for the current url
                const domain = domains.find(
                    (f) =>
                        f.id === urlObj.domain ||
                        (f.tenant && f.tenant.id) === urlObj.tenant
                );
                if (!domain) {
                    // domain not found
                    console.warn('Domain for current url is not found');
                    return false;
                }
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
