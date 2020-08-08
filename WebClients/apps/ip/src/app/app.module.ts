import { AdminAuthPagesModule } from '@admin/auth';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { IconDefinition } from '@ant-design/icons-angular';
import * as AllIcons from '@ant-design/icons-angular/icons';
import { NZ_ICONS } from 'ng-zorro-antd/icon';
import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { HttpBaseUrlInterceptorModule } from '@admin/common';
import { environment } from '../environments/environment';

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
        AdminAuthPagesModule,
        AppRoutingModule,
        HttpBaseUrlInterceptorModule.forRoot({ baseUrl: environment.baseUrl }),
    ],
    providers: [{ provide: NZ_ICONS, useValue: icons }],
    bootstrap: [AppComponent],
})
export class AppModule {}
