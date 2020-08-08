import { Location } from '@angular/common';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Inject, InjectionToken, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export const HTTP_BASE_URL_CONFIG = new InjectionToken<string>('HTTP_BASE_URL_CONFIG');

const join = (prefix: string, baseUrl: string, url: string) =>
    Location.joinWithSlash(baseUrl, url.substring(prefix.length));

const urlHasSchema = (url: string) =>
    url && (url.startsWith('http://') || url.startsWith('https://'));

const getBaseUrlByPrefix = (api: string, url: string) => {
    // consider api.url default base url if prefix is not defined
    return join('', api, url);
};

const getFullUrl = (config: string, url: string) => {
    // Is schema presented consider url is full
    if (!urlHasSchema(url)) {
        return getBaseUrlByPrefix(config, url);
    } else {
        return url;
    }
};

/**
 * Add base url to any relative requests url
 */
@Injectable()
export class HttpBaseUrlInterceptor implements HttpInterceptor {
    constructor(
        @Inject(HTTP_BASE_URL_CONFIG)
        private readonly config: string,
    ) {}

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const url = getFullUrl(this.config, request.url);
        if (!urlHasSchema(request.url)) {
            request = request.clone({
                url,
            });
        }

        return next.handle(request);
    }
}
