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
    AUTH_CLIENT_ID,
} from './constants';

export interface IAuthConfig {
    loginUrl: string;
    signupUrl: string;
    tokenUrl: string;
    logoutUrl: string;
    returnLoginUri: string;
    returnLoginPath?: string;
    returnLogoutUri: string;
    clientId: string;
    scope: string;
    stateStringLength: number;
    pkceCodeVerifierLength: number;
    refreshTokenUrl: string;
}

export const PERIMETER_AUTH_CONFIG = new InjectionToken(
    'PERIMETER_AUTH_CONFIG'
);

export interface AuthOptions {
    useSSO?: boolean;
    clientId?: string;
    // Path to redirect after successful login (without base url, it will be taken from returnUri)
    redirectPath?: string;
}

export type HashMap = { [key: string]: string | number };

interface JWTToken extends HashMap {
    iss: string;
    sub: string;
    aud: string;
    iat: number;
    exp: number;
}

const parseJwt = (token: string): JWTToken => {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
        atob(base64)
            .split('')
            .map(function (c) {
                return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
            })
            .join('')
    );

    return JSON.parse(jsonPayload);
};

/**
 * Get parsed token if it exists and not expired
 */
const validateToken = (token: string, validateExp = true): JWTToken | null => {
    if (token) {
        const jwtToken = parseJwt(token);
        console.log('token', jwtToken);
        if (validateExp) {
            if (!jwtToken.exp) {
                console.warn('token.exp is undefined');
                return null;
            }
            const now = new Date().getTime() / 1000;
            if (jwtToken.exp < now) {
                console.warn('token expired');
                return null;
            }
        }
        return jwtToken;
    } else {
        console.warn('idToken / accessToken not found');
        return null;
    }
};

@Injectable({ providedIn: 'root' })
export class AuthService {
    constructor(
        @Inject(PERIMETER_AUTH_CONFIG) private readonly config: IAuthConfig,
        private readonly http: HttpClient
    ) {}

    private async createPKCEAuthQuery(opts: AuthOptions = {}) {
        const codeVerifier = getRandomString(
            this.config.pkceCodeVerifierLength
        );

        const hashed = await getSHA256(codeVerifier);
        const codeChallenge = base64arrayEncode(hashed);
        const nonce = getRandomString(this.config.stateStringLength);
        const state = JSON.stringify({
            nonce,
            redirectPath: opts.redirectPath || this.config.returnLoginPath,
        });
        localStorage.setItem(AUTH_CODE_VERIFIER, codeVerifier);
        localStorage.setItem(AUTH_STATE, state);
        console.log('codeVerifier', codeVerifier);
        console.log('codeChallenge', codeChallenge);

        const clientId = opts.clientId || this.config.clientId;
        // TODO : Is it ok ???
        localStorage.setItem(AUTH_CLIENT_ID, clientId);
        return `client_id=${clientId}&response_type=code&state=${encodeURIComponent(
            state
        )}&redirect_uri=${encodeURI(
            this.config.returnLoginUri
        )}&scope=${encodeURI(
            this.config.scope
        )}&code_challenge=${codeChallenge}&code_challenge_method=S256${
            opts.useSSO ? '&prompt=none' : ''
        }`;
    }

    async createLoginUrl(opts: AuthOptions = {}) {
        const q = await this.createPKCEAuthQuery(opts);
        return `${this.config.loginUrl}?${q}`;
    }

    async createSignUpUrl(opts: AuthOptions = {}) {
        const q = await this.createPKCEAuthQuery(opts);
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
        const parsedState = JSON.parse(state);

        const clientId = localStorage.getItem(AUTH_CLIENT_ID);

        const payload = {
            grant_type: 'code',
            code,
            redirect_uri: this.config.returnLoginUri,
            client_id: clientId,
            code_verifier: sessionCodeVerifier,
        };
        try {
            const result = await this.http
                .post<TokensResult>(`${this.config.tokenUrl}`, payload)
                .toPromise();
            localStorage.setItem(ID_TOKEN, result.id_token);
            localStorage.setItem(ACCESS_TOKEN, result.access_token);
            localStorage.setItem(REFRESH_TOKEN, result.refresh_token);
            return parsedState.redirectPath;
        } finally {
            localStorage.removeItem(AUTH_CODE_VERIFIER);
            localStorage.removeItem(AUTH_STATE);
            localStorage.removeItem(AUTH_CLIENT_ID);
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

    /**
     * Will redirect to the IDP login page
     * @param opts
     */
    async authorize(opts: AuthOptions = {}) {
        const loginUrl = await this.createLoginUrl(opts);
        document.location.href = loginUrl;
    }

    /**
     * Validate id and optionally access tokens
     * @param validateExp - does need validate token expiration date
     * @param validateAccessToken  - does validate access token
     */
    validateTokens(
        validateExp = true,
        validateAccessToken = true
    ): JWTToken | null {
        const idToken = this.idToken;
        const jwtIdToken = validateToken(idToken, validateExp);
        if (!jwtIdToken) {
            console.warn('idToken invalid');
            return null;
        }
        if (validateAccessToken) {
            const accessToken = this.accessToken;
            const jwtAccessToken = validateToken(accessToken, validateExp);
            if (!jwtAccessToken) {
                console.warn('accessToken invalid');
                return null;
            }
        }
        return jwtIdToken;
    }

    get idToken() {
        return localStorage.getItem(ID_TOKEN);
    }

    get accessToken() {
        return localStorage.getItem(ACCESS_TOKEN);
    }

    logout() {
        const accessToken = this.accessToken;
        const url = `${this.config.logoutUrl}?return_uri=${encodeURI(
            this.config.returnLogoutUri
        )}&access_token=${accessToken}`;
        this.resetTokens();
        document.location.href = url;
    }

    async refreshToken() {
        const refreshToken = localStorage.getItem(REFRESH_TOKEN);
        if (!refreshToken) {
            return false;
        }
        try {
            const result = await this.http
                .post<TokensResult>(this.config.refreshTokenUrl, {
                    refreshToken,
                })
                .toPromise();
            localStorage.setItem(ID_TOKEN, result.id_token);
            localStorage.setItem(ACCESS_TOKEN, result.access_token);
            localStorage.setItem(REFRESH_TOKEN, result.refresh_token);
            return true;
        } catch {
            return false;
        }
    }

    resetTokens() {
        localStorage.removeItem(ID_TOKEN);
        localStorage.removeItem(ACCESS_TOKEN);
        localStorage.removeItem(REFRESH_TOKEN);
    }
}
