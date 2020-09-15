import {
    HttpErrorResponse,
    HttpEvent,
    HttpHandler,
    HttpInterceptor,
    HttpRequest,
} from '@angular/common/http';
import { Inject, Injectable, InjectionToken } from '@angular/core';
import { from, Observable, of, throwError } from 'rxjs';
import { catchError, filter, flatMap, map, switchMap } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { ACCESS_TOKEN } from './constants';

export const PERIMETER_AUTH_APP_BASE_URL = new InjectionToken(
    'PERIMETER_AUTH_APP_BASE_URL'
);

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
            const request1 = request.clone({
                headers: request.headers.append(
                    'Authorization',
                    `Bearer ${accessToken}`
                ),
            });

            // Possible token is expired refresh one
            return next.handle(request1).pipe(
                catchError((err) => {
                    if (
                        err instanceof HttpErrorResponse &&
                        err.status === 401
                    ) {
                        return from(this.authService.refreshToken()).pipe(
                            catchError(() => throwError(err)),
                            switchMap((f) => {
                                const accessTokenRefreshed = localStorage.getItem(
                                    ACCESS_TOKEN
                                );
                                const request2 = request.clone({
                                    headers: request.headers.append(
                                        'Authorization',
                                        `Bearer ${accessTokenRefreshed}`
                                    ),
                                });
                                return f
                                    ? next.handle(request2)
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
