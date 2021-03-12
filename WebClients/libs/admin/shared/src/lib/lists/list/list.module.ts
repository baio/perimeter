import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';
import {
    HlcNzCustomCellDirective,
    HlcNzImgColumnConfig,
    HlcNzTableModule,
    HLC_NZ_IMG_COLUMN_CONFIG,
    HLC_NZ_TABLE_CELL_MAP,
} from '@nz-holistic/nz-list';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { AdminListComponent } from './list.component';
import {
    tableCellMap,
    tableCellMapComponents,
    tableCellMapModules,
} from './table-cell-map';

@NgModule({
    declarations: [AdminListComponent],
    exports: [AdminListComponent, HlcNzCustomCellDirective],
    imports: [
        BrowserModule,
        HlcNzTableModule,
        RouterModule,
        NzAlertModule,
        ...tableCellMapModules,
        TranslocoModule,
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
