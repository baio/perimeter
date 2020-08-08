import {
    HttpEvent,
    HttpHandler,
    HttpInterceptor,
    HttpRequest,
} from '@angular/common/http';
import { Inject, Injectable, InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { ACCESS_TOKEN } from './constants';

export const PERIMETER_AUTH_APP_BASE_URL = new InjectionToken(
    'PERIMETER_AUTH_APP_BASE_URL'
);

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    constructor(
        @Inject(PERIMETER_AUTH_APP_BASE_URL) private readonly appBaseUrl: string
    ) {}

    intercept(
        request: HttpRequest<any>,
        next: HttpHandler
    ): Observable<HttpEvent<any>> {
        const url = request.url;
        const accessToken = sessionStorage.getItem(ACCESS_TOKEN);
        const isTheSameBaseUrl = url.startsWith(this.appBaseUrl);
        if (accessToken && isTheSameBaseUrl) {
            request = request.clone({
                headers: request.headers.append(
                    'Authorization',
                    `Bearer ${accessToken}`
                ),
            });
            // TODO : Refresh on token expired
        }

        return next.handle(request);
    }
}
