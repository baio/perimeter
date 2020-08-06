import { Injectable, INJECTOR, InjectionToken, Inject } from '@angular/core';
import { getRandomString, getSHA256, base64arrayEncode } from './utils';
import { HttpClient } from '@angular/common/http';
import { LoginData, LoginParams } from './models';

export interface IPAuthConfig {
    baseUrl: string;
    defaultLoginPath: string;
    loginPath: string;
    tokenPath: string;
    logoutPath: string;
    redirectUri: string;
    scope: string;
    clientId: string;
}

export const IP_AUTH_CONFIG = new InjectionToken(
    'IP_PROVIDER_AUTH_CONFIG'
);

export interface LoginPassword {
    email: string;
    password: string;
}

@Injectable({ providedIn: 'root' })
export class IPAuthService {
    constructor(
        @Inject(IP_AUTH_CONFIG)
        private readonly config: IPAuthConfig,
        private readonly http: HttpClient
    ) {}

    async login(prms: LoginParams, data: LoginPassword) {
        const payload: LoginData = {
            ...prms,
            ...data,
        };
        return this.http.post(
            `${this.config.baseUrl}/${this.config.defaultLoginPath}`,
            payload
        );
    }

    /*
    async login(data: LoginPassword) {
        const codeVerifier = getRandomString(128);
        const sha256 = await getSHA256(codeVerifier);
        const codeChallenge = base64arrayEncode(sha256);
        const state = getRandomString(10);
        sessionStorage.setItem('auth_state', state);
        const payload: LoginData = {
            response_type: 'code',
            client_id: this.config.clientId,
            state,
            redirect_uri: encodeURI(this.config.redirectUri),
            scope: encodeURI(this.config.scope),
            email: data.email,
            password: data.password,
            code_challenge: codeChallenge,
            code_challenge_method: 'S256',
        };
        return this.http.post(
            `${this.config.baseUrl}/${this.config.defaultLoginPath}`,
            payload
        );
    }
    */
}
