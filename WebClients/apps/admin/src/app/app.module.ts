import { AdminDomainsModule } from '@admin/domains';
import { AdminProfileModule } from '@admin/profile';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { IconDefinition } from '@ant-design/icons-angular';
import * as AllIcons from '@ant-design/icons-angular/icons';
import { EffectsModule } from '@ngrx/effects';
import { Store, StoreModule } from '@ngrx/store';
import { StoreDevtoolsModule } from '@ngrx/store-devtools';
import { HlcNzFormModule } from '@nz-holistic/nz-forms';
import {
    HttpBaseUrlInterceptorModule,
    HttpErrorMessageInterceptorModule,
} from '@perimeter/common';
import { AuthModule } from '@perimeter/ngx-auth';
import { loadInfo } from 'libs/admin/shared/src/lib/info/ngrx/actions';
import { en_US, NZ_I18N } from 'ng-zorro-antd/i18n';
import { NZ_ICONS } from 'ng-zorro-antd/icon';
import { NzNotificationModule } from 'ng-zorro-antd/notification';
import { InfoModule } from '../../../../libs/admin/shared/src/lib/info';
import { environment } from '../environments/environment';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AuthRoutingModule } from './auth-routing.module';
import { TranslocoRootModule } from './transloco-root.module';

const antDesignIcons = AllIcons as {
    [key: string]: IconDefinition;
};
const icons: IconDefinition[] = Object.keys(antDesignIcons).map(
    (key) => antDesignIcons[key]
);

@NgModule({
    declarations: [AppComponent],
    imports: [
        BrowserModule,
        HttpClientModule,
        AppRoutingModule,
        AuthRoutingModule,
        AdminDomainsModule,
        HlcNzFormModule.forRoot(),
        RouterModule,
        AuthModule.forRoot(environment.baseUrl, environment.auth),
        HttpBaseUrlInterceptorModule.forRoot({ baseUrl: environment.baseUrl }),
        AdminProfileModule,
        StoreModule.forRoot({}),
        EffectsModule.forRoot(),
        StoreDevtoolsModule.instrument({
            logOnly: environment.production,
        }),
        // Important ! Always last in order to not mess with other interceptors !
        HttpErrorMessageInterceptorModule,
        NzNotificationModule,
        InfoModule,
        TranslocoRootModule,
    ],
    providers: [
        { provide: NZ_ICONS, useValue: icons },
        {
            provide: NZ_I18N,
            useValue: en_US,
        },
    ],
    bootstrap: [AppComponent],
})
export class AppModule {
    constructor(store: Store) {
        store.dispatch(loadInfo());
    }
}
