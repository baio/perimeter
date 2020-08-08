import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Inject, Injectable, InjectionToken } from '@angular/core';
import { LoginResult, LoginResultErrorType, TokensResult } from './models';
import { base64arrayEncode, getRandomString, getSHA256 } from './utils';
import {
    AUTH_CODE_VERIFIER,
    AUTH_STATE,
    ID_TOKEN,
    ACCESS_TOKEN,
    REFRESH_TOKEN,
} from './constants';

export interface IAuthConfig {
    baseUrl: string;
    loginPath: string;
    tokenUrl: string;
    logoutPath: string;
    returnUri: string;
    clientId: string;
    scope: string;
    stateStringLength: number;
    pkceRandomStringLength: number;
}

export const PERIMETER_AUTH_CONFIG = new InjectionToken(
    'PERIMETER_AUTH_CONFIG'
);

@Injectable({ providedIn: 'root' })
export class AuthService {
    constructor(
        @Inject(PERIMETER_AUTH_CONFIG) private readonly config: IAuthConfig,
        private readonly http: HttpClient
    ) {}

    async createLoginUrl() {
        const codeVerifier = getRandomString(
            this.config.pkceRandomStringLength
        );
        const hashed = await getSHA256(codeVerifier);
        const codeChallenge = base64arrayEncode(hashed);
        const state = getRandomString(this.config.stateStringLength);

        sessionStorage.setItem(AUTH_CODE_VERIFIER, codeVerifier);
        sessionStorage.setItem(AUTH_STATE, state);

        return `${this.config.baseUrl}/${this.config.loginPath}?client_id=${
            this.config.clientId
        }&response_type=code&state=${state}&redirect_uri=${encodeURI(
            this.config.returnUri
        )}&scope=${encodeURI(
            this.config.scope
        )}&code_challenge=${codeChallenge}&code_challenge_method=S256`;
    }

    async token(code: string, state: string) {
        const sessionCodeVerifier = sessionStorage.getItem(AUTH_CODE_VERIFIER);
        const sessionState = sessionStorage.getItem(AUTH_STATE);
        if (!sessionStorage) {
            throw new Error('Session code_verifier not found');
        }
        if (!sessionState) {
            throw new Error('Session state not found');
        }
        if (state !== sessionState) {
            throw new Error('Session state not match');
        }
        const payload = {
            grant_type: 'code',
            code,
            redirect_uri: this.config.returnUri,
            client_id: this.config.clientId,
            code_verifier: sessionCodeVerifier,
        };
        try {
            const result = await this.http
                .post<TokensResult>(`${this.config.tokenUrl}`, payload)
                .toPromise();
            sessionStorage.setItem(ID_TOKEN, result.id_token);
            sessionStorage.setItem(ACCESS_TOKEN, result.access_token);
            localStorage.setItem(REFRESH_TOKEN, result.refresh_token);
        } finally {
            sessionStorage.removeItem(AUTH_CODE_VERIFIER);
            sessionStorage.removeItem(AUTH_STATE);
        }
    }

    parseLoginRedirect(prms: { [key: string]: string }): LoginResult {
        if (prms['error']) {
            return {
                kind: 'error',
                error: prms['error'] as LoginResultErrorType,
                description: prms['error_description'],
                uri: prms['error_uri'],
            };
        } else if (prms['code']) {
            return {
                kind: 'ok',
                code: prms['code'],
                state: prms['state'],
            };
        } else {
            return {
                kind: 'error',
                error: 'code_not_found',
            };
        }
    }
}
