import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { NzDividerModule } from 'ng-zorro-antd/divider';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { AdminPermissionsColumnComponent } from './permissions-column.component';

@NgModule({
    imports: [CommonModule, NzIconModule, NzDividerModule, NzTagModule],
    declarations: [AdminPermissionsColumnComponent],
    exports: [AdminPermissionsColumnComponent],
})
export class AdminPermissionsColumnModule {
    constructor() {}
}
