import { ModuleWithProviders, NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { IAuthConfig, PERIMETER_AUTH_CONFIG } from './auth.service';
import {
    PERIMETER_AUTH_APP_BASE_URL,
    AuthInterceptor,
} from './auth.interceptor';

@NgModule({
    declarations: [],
    imports: [CommonModule],
})
export class AuthModule {
    static forRoot(
        appBaseUrl: string,
        config: IAuthConfig
    ): ModuleWithProviders<any> {
        return {
            ngModule: AuthModule,
            providers: [
                {
                    provide: PERIMETER_AUTH_CONFIG,
                    useValue: config,
                },
                {
                    provide: PERIMETER_AUTH_APP_BASE_URL,
                    useValue: appBaseUrl,
                },
                {
                    provide: HTTP_INTERCEPTORS,
                    multi: true,
                    useClass: AuthInterceptor,
                },
            ],
        };
    }
}
