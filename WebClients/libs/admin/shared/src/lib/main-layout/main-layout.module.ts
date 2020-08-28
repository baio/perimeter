import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';
import { MainLayoutComponent } from './main-layout.component';
import { NzLayoutModule } from 'ng-zorro-antd/layout';
import { NzBreadCrumbModule } from 'ng-zorro-antd/breadcrumb';
import { NzMenuModule } from 'ng-zorro-antd/menu';
import { NzDropDownModule, NzIconModule } from 'ng-zorro-antd';

@NgModule({
    imports: [
        CommonModule,
        BrowserModule,
        BrowserAnimationsModule,
        RouterModule,
        NzLayoutModule,
        NzBreadCrumbModule,
        NzMenuModule,
        NzDropDownModule,
        NzIconModule,
    ],
    declarations: [MainLayoutComponent],
    exports: [],
})
export class MainLayoutModule {}
