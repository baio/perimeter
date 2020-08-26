import { createAction, props } from '@ngrx/store';
import { User, Domain } from './models';

export const authenticate = createAction('[Admin Profile] Authenticate');

export const authenticationFails = createAction(
    '[Admin Profile] Authentication Fails'
);

export const authenticationSuccess = createAction(
    '[Admin Profile] Authentication Success',
    props<{ user: User }>()
);

export const profileLoaded = createAction(
    '[Admin Profile] Profile Loaded',
    props<{ domains: Domain[] }>()
);
