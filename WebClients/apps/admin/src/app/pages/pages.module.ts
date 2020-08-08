import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzCheckboxModule } from 'ng-zorro-antd/checkbox';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { RouterModule } from '@angular/router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HomePageComponent } from './home-page/home-page.component';
import { BrowserModule } from '@angular/platform-browser';
import { LoginCbPageComponent } from './login-cb-page/login-cb-page.component';

@NgModule({
    imports: [
        CommonModule,
        BrowserModule,
        BrowserAnimationsModule,
        RouterModule,
        NzFormModule,
        NzInputModule,
        NzButtonModule,
        NzIconModule,
        NzCheckboxModule,
        NzSelectModule,
        NzModalModule,
        NzAlertModule,
    ],
    declarations: [HomePageComponent, LoginCbPageComponent],
    exports: [],
})
export class AdminPagesModule {}
