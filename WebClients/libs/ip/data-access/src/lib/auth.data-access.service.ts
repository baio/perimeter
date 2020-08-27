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

@Injectable()
export class AuthDataAccessService {
    constructor(private readonly http: HttpClient) {}

    assignSSO() {
        return this.http.get('http://localhost:4201/auth/assign-sso', {
            withCredentials: true,
        });
    }

    signUp(data: SignUpData, queryString: string): Observable<any> {
        const payload = {
            ...data,
            queryString,
        };
        return this.http.post('auth/sign-up', payload);
    }

    signUpConfirm(token: string): Observable<any> {
        return this.http.post('auth/sign-up/confirm', { token });
    }

    resetPassword(data: ResetPasswordData): Observable<any> {
        return this.http.post('auth/reset-password', data);
    }

    resetPasswordConfirm(data: ResetPasswordConfirmData): Observable<any> {
        return this.http.post('auth/reset-password/confirm', data);
    }
}
