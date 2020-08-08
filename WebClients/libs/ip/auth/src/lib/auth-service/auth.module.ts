import { NgModule, ModuleWithProviders } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { IPAuthConfig, AUTH_CONFIG } from './auth.service';

@NgModule({
    imports: [HttpClientModule],
    providers: [],
    declarations: [],
    exports: [],
})
export class AuthModule {
    static forRoot(config: IPAuthConfig): ModuleWithProviders<any> {
        return {
            ngModule: AuthModule,
            providers: [
                {
                    provide: AUTH_CONFIG,
                    useValue: config,
                },
            ],
        };
    }
}
