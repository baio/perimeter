import { ModuleWithProviders, NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import {
    HTTP_BASE_URL_CONFIG,
    HttpBaseUrlInterceptor,
} from './http-base-url.interceptor';

@NgModule({
    declarations: [],
    imports: [CommonModule],
})
export class HttpBaseUrlInterceptorModule {
    static forRoot(config: { baseUrl: string }): ModuleWithProviders<any> {
        return {
            ngModule: HttpBaseUrlInterceptorModule,
            providers: [
                {
                    provide: HTTP_BASE_URL_CONFIG,
                    useValue: config.baseUrl,
                },
                {
                    provide: HTTP_INTERCEPTORS,
                    multi: true,
                    useClass: HttpBaseUrlInterceptor,
                },
            ],
        };
    }
}
