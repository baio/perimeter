import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzGridModule } from 'ng-zorro-antd/grid';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzPageHeaderModule } from 'ng-zorro-antd/page-header';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzSwitchModule } from 'ng-zorro-antd/switch';
import { InfoModule } from '../../info';
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
        InfoModule,
    ],
})
export class AdminListHeaderModule {}
