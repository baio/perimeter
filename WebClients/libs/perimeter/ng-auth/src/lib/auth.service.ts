import { Injectable } from '@angular/core';

export interface AuthConfig {
    baseUrl: string;
    loginPath: string;
    tokenPath: string;
    logoutPath: string;
    returnUrl: string;
    clientId?: string;
}

@Injectable()
export class Auth {
    login(clientId?: string) {

    }
}