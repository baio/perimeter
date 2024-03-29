import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { IconDefinition } from '@ant-design/icons-angular';
import * as AllIcons from '@ant-design/icons-angular/icons';
import { NZ_ICONS } from 'ng-zorro-antd/icon';
import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import {
    HttpBaseUrlInterceptorModule,
    HttpErrorMessageInterceptorModule,
} from '@perimeter/common';
import { environment } from '../environments/environment';
import { AuthModule } from '@perimeter/ngx-auth';

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
        HttpErrorMessageInterceptorModule,
        HttpBaseUrlInterceptorModule.forRoot({ baseUrl: environment.baseUrl }),
        AuthModule.forRoot(environment.baseUrl, environment.auth),
    ],
    providers: [{ provide: NZ_ICONS, useValue: icons }],
    bootstrap: [AppComponent],
})
export class AppModule {}
