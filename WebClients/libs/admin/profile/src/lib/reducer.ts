import { createReducer, on } from '@ngrx/store';
import {
    authenticate,
    authenticationSuccess,
    profileLoadSuccess,
    authenticationFails,
    profileLoadFails,
    loadManagementDomainsSuccess,
} from './actions';
import { ProfileState } from './models';
import { pipe, map, fromPairs } from 'lodash/fp';

export const initialState: ProfileState = {
    status: 'init',
    user: null,
    domains: {},
};

const listToNKeyHash: <T extends { id: number }>(
    list: T[]
) => { [key: number]: T } = pipe(
    map((x) => [x.id, x]),
    fromPairs
);

const _profileReducer = createReducer(
    initialState,
    on(authenticate, (state) => ({ ...state, status: 'authenticating' })),
    on(authenticationSuccess, (state, { user }) => ({ ...state, user })),
    on(authenticationFails, (state) => ({
        ...state,
        status: 'unAuthenticated',
    })),
    on(profileLoadSuccess, (state, { domains }) => ({
        ...state,
        status: 'success',
        domains: listToNKeyHash(domains),
    })),
    on(loadManagementDomainsSuccess, (state, { domains }) => ({
        ...state,
        domains: listToNKeyHash(domains),
    })),
    on(profileLoadFails, (state) => ({
        ...state,
        status: 'error',
    }))
);

export const profileReducer = (state, action) => {
    return _profileReducer(state, action);
};
