import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ProfileState, Domain, ProfileStatus } from './models';
import { values, includes } from 'lodash/fp';
import { Observable } from 'rxjs';
import { skipWhile } from 'rxjs/operators';

export const selectProfile = createFeatureSelector<ProfileState>('profile');

export const selectStatus = createSelector(selectProfile, (x) => x.status);

export type CompletedProfileStatus = 'notAuthenticated' | 'success' | 'error';

export const filterCompletedStatuses$ = (obs$: Observable<ProfileStatus>) =>
    obs$.pipe(
        skipWhile((x) => includes(x, ['init', 'authenticating']))
    ) as Observable<CompletedProfileStatus>;

export const selectUser = createSelector(selectProfile, (x) => x.user);

export const selectProfileDomains = createSelector(selectProfile, (x) =>
    x ? x.domains : {}
);

export const selectProfileDomainsList = createSelector<any, any, Domain[]>(
    selectProfileDomains,
    values
);

//
export const getObjIdFromUrl = (url: string) => {
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

export const getActiveDomain = (url: string) => (domains: Domain[]) => {
    const urlObj = getObjIdFromUrl(url);
    return domains.find(
        (f) =>
            f.id === urlObj.domain ||
            (f.tenant && f.tenant.id) === urlObj.tenant
    );
};

export const selectActiveDomain = (url: string) =>
    createSelector<any, any, Domain | null>(
        selectProfileDomainsList,
        getActiveDomain(url)
    );
