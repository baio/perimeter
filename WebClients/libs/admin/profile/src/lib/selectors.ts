import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ProfileState, Domain } from './models';
import { values } from 'lodash/fp';

export const selectProfile = createFeatureSelector<ProfileState>('profile');

export const selectStatus = createSelector(selectProfile, (x) => x.status);

export const selectUser = createSelector(selectProfile, (x) => x.user);

export const selectProfileDomains = createSelector(selectProfile, (x) =>
    x ? x.domains : {}
);

export const selectProfileDomainsList = createSelector<any, any, Domain[]>(
    selectProfileDomains,
    values
);
