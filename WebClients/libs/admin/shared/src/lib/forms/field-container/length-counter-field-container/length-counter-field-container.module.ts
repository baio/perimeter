import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { HlcNzInputContainerModule } from '@nz-holistic/nz-forms';

import { AdminLengthCounterContainerComponent } from './length-counter-field-container.component';

@NgModule({
    imports: [
        CommonModule,
        HlcNzInputContainerModule,
    ],
    declarations: [AdminLengthCounterContainerComponent],
    exports: [AdminLengthCounterContainerComponent],
})
export class AdminLengthCounterFieldContainerModule {
    constructor() {}
}
