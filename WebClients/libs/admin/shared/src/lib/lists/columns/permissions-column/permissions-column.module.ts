import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { NzIconModule, NzDividerModule, NzTagModule } from 'ng-zorro-antd';
import { AdminPermissionsColumnComponent } from './permissions-column.component';

@NgModule({
    imports: [CommonModule, NzIconModule, NzDividerModule, NzTagModule],
    declarations: [AdminPermissionsColumnComponent],
    exports: [AdminPermissionsColumnComponent],
})
export class AdminPermissionsColumnModule {
    constructor() {}
}
