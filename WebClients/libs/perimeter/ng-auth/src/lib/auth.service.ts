import { Injectable, InjectionToken, Inject } from '@angular/core';
import { getRandomString, getSHA256, base64arrayEncode } from './utils';
import { HttpClient } from '@angular/common/http';
import { throwError } from 'rxjs';
import { tap } from 'rxjs/operators';
import { AuthResult } from './models';

export interface IAuthConfig {
    baseUrl: string;
    loginPath: string;
    tokenPath: string;
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
        const sha256 = await getSHA256(codeVerifier);
        const codeChallenge = base64arrayEncode(sha256);
        const state = btoa(getRandomString(this.config.stateStringLength));

        sessionStorage.setItem('auth_code_verifier', codeVerifier);
        sessionStorage.setItem('auth_state', state);

        return `${this.config.baseUrl}/${this.config.loginPath}?client_id=${
            this.config.clientId
        }&response_type=code&state=${state}&redirect_uri=${encodeURI(
            this.config.returnUri
        )}&scope=${encodeURI(
            this.config.scope
        )}&code_challenge=${codeChallenge}&code_challenge_method=S256`;
    }

    async token(code: string, state: string) {
        const sessionCodeVerifier = sessionStorage.getItem(
            'auth_code_verifier'
        );
        const sessionState = sessionStorage.getItem('auth_state');
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
                .post<AuthResult>(
                    `${this.config.baseUrl}/${this.config.tokenPath}`,
                    payload
                )
                .toPromise();
            sessionStorage.setItem('id_token', result.id_token);
            sessionStorage.setItem('access_token', result.access_token);
            localStorage.setItem('refresh_token', result.refresh_token);
        } finally {
            sessionStorage.removeItem('auth_code_verifier');
            sessionStorage.removeItem('auth_state');
        }
    }
}
