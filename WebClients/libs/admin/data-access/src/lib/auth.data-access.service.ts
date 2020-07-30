import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, never } from 'rxjs';

export interface SignUpData {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
}

@Injectable()
export class AuthDataAccessService {
    constructor(private readonly http: HttpClient) {}

    signUp(data: SignUpData): Observable<any> {
        return this.http.post('auth/sign-up', data);
    }

    signUpConfirm(token: string): Observable<any> {
        return this.http.post('auth/sign-up/confirm', { token });
    }
}
