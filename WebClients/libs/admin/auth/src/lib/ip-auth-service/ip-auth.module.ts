import { NgModule, ModuleWithProviders } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { IPAuthConfig, IP_AUTH_CONFIG } from './ip-auth.service';

@NgModule({
    imports: [HttpClientModule],
    providers: [],
    declarations: [],
    exports: [],
})
export class IPAuthModule {
    static forRoot(config: IPAuthConfig): ModuleWithProviders<any> {
        return {
            ngModule: IPAuthModule,
            providers: [
                {
                    provide: IP_AUTH_CONFIG,
                    useValue: config,
                },
            ],
        };
    }
}
