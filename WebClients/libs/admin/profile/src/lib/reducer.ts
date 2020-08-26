import { createReducer, on } from '@ngrx/store';
import { authenticate, authenticationSuccess, profileLoaded } from './actions';
import { ProfileState } from './models';
import { pipe, map, fromPairs } from 'lodash/fp';

export const initialState: ProfileState = {
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
    on(authenticationSuccess, (state, { user }) => ({ ...state, user })),
    on(profileLoaded, (state, { domains }) => ({
        ...state,
        domains: listToNKeyHash(domains),
    }))
);

export const profileReducer = (state, action) => {
    return _profileReducer(state, action);
};
