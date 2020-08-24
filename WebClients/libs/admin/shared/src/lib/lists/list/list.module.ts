import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import {
    HlcNzTableModule,
    HLC_NZ_TABLE_CELL_MAP,
    HLC_NZ_IMG_COLUMN_CONFIG,
    HlcNzImgColumnConfig,
    HlcNzCustomCellDirective,
} from '@nz-holistic/nz-list';
import { AdminListComponent } from './list.component';
import {
    tableCellMap,
    tableCellMapComponents,
    tableCellMapModules,
} from './table-cell-map';
import { NzAlertModule } from 'ng-zorro-antd';

@NgModule({
    declarations: [AdminListComponent],
    exports: [AdminListComponent, HlcNzCustomCellDirective],
    imports: [
        BrowserModule,
        HlcNzTableModule,
        RouterModule,
        NzAlertModule,
        ...tableCellMapModules,
    ],
    providers: [
        {
            provide: HLC_NZ_TABLE_CELL_MAP,
            useValue: tableCellMap,
            multi: true,
        },
        {
            provide: HLC_NZ_IMG_COLUMN_CONFIG,
            useValue: {
                placeholderUrl: '/assets/images/img-placeholder.png',
            } as HlcNzImgColumnConfig,
        },
    ],
    entryComponents: [...tableCellMapComponents],
})
export class AdminListModule {}
