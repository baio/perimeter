import { ModuleWithProviders, NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { IAuthConfig, PERIMETER_AUTH_CONFIG } from './auth.service';

@NgModule({
    declarations: [],
    imports: [CommonModule],
})
export class AuthModule {
    static forRoot(config: IAuthConfig): ModuleWithProviders<any> {
        return {
            ngModule: AuthModule,
            providers: [
                {
                    provide: PERIMETER_AUTH_CONFIG,
                    useValue: config,
                },
            ],
        };
    }
}
