import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface SignUpData {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
}

export interface ResetPasswordData {
    email: string;
}

export interface ResetPasswordConfirmData {
    token: string;
    password: string;
}

export interface LoginParams {
    client_id: string;
    response_type: 'code';
    state: string;
    redirect_uri: string;
    scope: string;
    code_challenge: string;
    code_challenge_method: 'S256';
}

export interface LoginPassword {
    email: string;
    password: string;
}

export interface AppInfo {
    title: string;
    socialConnections: string[];
}

@Injectable()
export class AuthDataAccessService {
    constructor(private readonly http: HttpClient) {}

    signUp(data: SignUpData, returnUrl: string): Observable<any> {
        const payload = {
            ...data,
            returnUrl,
        };
        return this.http.post('auth/sign-up', payload);
    }

    signUpConfirm(token: string): Observable<any> {
        return this.http.post('auth/sign-up/confirm', { token });
    }

    resetPassword(data: ResetPasswordData, returnUrl: string): Observable<any> {
        const payload = {
            ...data,
            returnUrl,
        };
        return this.http.post('auth/reset-password', payload);
    }

    resetPasswordConfirm(data: ResetPasswordConfirmData): Observable<any> {
        return this.http.post('auth/reset-password/confirm', data);
    }

    getAppInfo(clientId: string): Observable<any> {
        return this.http.get<AppInfo>(`auth/applications/${clientId}`);
    }
}
