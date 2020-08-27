import { createAction, props } from '@ngrx/store';
import { User, Domain } from './models';
import { HttpErrorResponse } from '@angular/common/http';

export const authenticate = createAction('[Admin Profile] Authenticate');

export const authenticationFails = createAction(
    '[Admin Profile] Authentication Fails'
);

export const authenticationSuccess = createAction(
    '[Admin Profile] Authentication Success',
    props<{ user: User }>()
);

export const profileLoadSuccess = createAction(
    '[Admin Profile] Profile Load Success',
    props<{ domains: Domain[] }>()
);

export const profileLoadFails = createAction(
    '[Admin Profile] Profile Load Fails',
    props<{ err: HttpErrorResponse }>()
);
