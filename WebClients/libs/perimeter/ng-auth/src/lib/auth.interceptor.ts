import {
    HttpErrorResponse,
    HttpEvent,
    HttpHandler,
    HttpInterceptor,
    HttpRequest,
} from '@angular/common/http';
import { Inject, Injectable, InjectionToken } from '@angular/core';
import { from, Observable, throwError } from 'rxjs';
import { catchError, finalize, switchMap } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { ACCESS_TOKEN } from './constants';

export const PERIMETER_AUTH_APP_BASE_URL = new InjectionToken(
    'PERIMETER_AUTH_APP_BASE_URL'
);

const nextHandle = (request: HttpRequest<any>, next: HttpHandler) => {
    const accessToken = localStorage.getItem(ACCESS_TOKEN);
    const request1 = request.clone({
        headers: request.headers.append(
            'Authorization',
            `Bearer ${accessToken}`
        ),
    });
    return next.handle(request1);
};

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    constructor(
        @Inject(PERIMETER_AUTH_APP_BASE_URL)
        private readonly appBaseUrl: string,
        private readonly authService: AuthService
    ) {}

    intercept(
        request: HttpRequest<any>,
        next: HttpHandler
    ): Observable<HttpEvent<any>> {
        const url = request.url;
        // TODO
        const accessToken = localStorage.getItem(ACCESS_TOKEN);
        const hasSchema =
            url.startsWith('http://') || url.startsWith('https://');
        const isTheSameBaseUrl = url.startsWith(this.appBaseUrl);
        if (accessToken && (!hasSchema || isTheSameBaseUrl)) {
            // Possible token is expired refresh one
            return nextHandle(request, next).pipe(
                catchError((err) => {
                    if (
                        err instanceof HttpErrorResponse &&
                        err.status === 401 &&
                        !this.authService.isRefreshTokenUrl(request.url)
                    ) {
                        return from(this.authService.refreshToken()).pipe(
                            catchError(() => throwError(err)),
                            switchMap((f) => {
                                return f
                                    ? nextHandle(request, next)
                                    : throwError(err);
                            })
                        );
                    } else {
                        return throwError(err);
                    }
                })
            );
        } else {
            return next.handle(request);
        }
    }
}
