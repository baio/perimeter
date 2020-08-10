import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { HlcNzDisplayModule } from '@nz-holistic/nz-controls';
import { NzTypographyModule } from 'ng-zorro-antd';

import { AdminDateTimeDisplayComponent } from './date-time-display.component';

@NgModule({
    imports: [
        CommonModule,
        HlcNzDisplayModule,
        NzTypographyModule,
    ],
    declarations: [AdminDateTimeDisplayComponent],
    exports: [AdminDateTimeDisplayComponent]
})
export class AdminDateTimeDisplayModule {
    constructor() {}
}
