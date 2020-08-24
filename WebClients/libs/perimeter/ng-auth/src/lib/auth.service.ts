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
    loginUrl: string;
    signupUrl: string;
    tokenUrl: string;
    logoutUrl: string;
    returnLoginUri: string;
    returnLogoutUri: string;
    clientId: string;
    scope: string;
    stateStringLength: number;
    pkceCodeVerifierLength: number;
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

    private async createPKCEAuthQuery(useSSO: boolean) {
        const codeVerifier = getRandomString(
            this.config.pkceCodeVerifierLength
        );

        const hashed = await getSHA256(codeVerifier);
        const codeChallenge = base64arrayEncode(hashed);
        const state = getRandomString(this.config.stateStringLength);

        localStorage.setItem(AUTH_CODE_VERIFIER, codeVerifier);
        localStorage.setItem(AUTH_STATE, state);
        console.log('codeVerifier', codeVerifier);
        console.log('codeChallenge', codeChallenge);

        return `client_id=${
            this.config.clientId
        }&response_type=code&state=${state}&redirect_uri=${encodeURI(
            this.config.returnLoginUri
        )}&scope=${encodeURI(
            this.config.scope
        )}&code_challenge=${codeChallenge}&code_challenge_method=S256${
            useSSO ? '&prompt=none' : ''
        }`;
    }

    async createLoginUrl(useSSO = false) {
        const q = await this.createPKCEAuthQuery(useSSO);
        return `${this.config.loginUrl}?${q}`;
    }

    async createSignUpUrl() {
        const q = await this.createPKCEAuthQuery(false);
        return `${this.config.signupUrl}?${q}`;
    }

    async token(code: string, state: string) {
        const sessionCodeVerifier = localStorage.getItem(AUTH_CODE_VERIFIER);
        const sessionState = localStorage.getItem(AUTH_STATE);
        if (!sessionCodeVerifier) {
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
            redirect_uri: this.config.returnLoginUri,
            client_id: this.config.clientId,
            code_verifier: sessionCodeVerifier,
        };
        try {
            const result = await this.http
                .post<TokensResult>(`${this.config.tokenUrl}`, payload)
                .toPromise();
            sessionStorage.setItem(ID_TOKEN, result.idToken);
            sessionStorage.setItem(ACCESS_TOKEN, result.accessToken);
            localStorage.setItem(REFRESH_TOKEN, result.refreshToken);
        } finally {
            localStorage.removeItem(AUTH_CODE_VERIFIER);
            localStorage.removeItem(AUTH_STATE);
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

    logout() {
        /*
        const form = document.createElement('form');
        form.setAttribute('method', 'post');
        form.setAttribute('action', this.config.logoutUrl);
        const inputReturnUri = document.createElement('input'); //input element, text
        inputReturnUri.setAttribute('type', 'hidden');
        inputReturnUri.setAttribute('returnUri', this.config.returnLogoutUri);
        form.appendChild(inputReturnUri);
        document.body.appendChild(form);
        form.submit();        
        */
        const accessToken = sessionStorage.getItem(ACCESS_TOKEN);
        const url = `${this.config.logoutUrl}?return_uri=${encodeURI(this.config.returnLogoutUri)}&access_token=${accessToken}`;
        document.location.href = url;
    }
}
