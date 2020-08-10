import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import {
    NzButtonModule,
    NzGridModule,
    NzIconModule,
    NzInputModule,
    NzPageHeaderModule,
    NzSelectModule,
    NzSwitchModule,
} from 'ng-zorro-antd';

import { AdminListHeaderComponent } from './list-header.component';

@NgModule({
    declarations: [AdminListHeaderComponent],
    exports: [AdminListHeaderComponent],
    imports: [
        BrowserModule,
        FormsModule,
        NzButtonModule,
        NzGridModule,
        NzIconModule,
        NzInputModule,
        NzPageHeaderModule,
        NzSelectModule,
        NzSwitchModule,
        ReactiveFormsModule,
    ],
})
export class AdminListHeaderModule {}
