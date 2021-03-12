import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { HlcNzInputContainerModule } from '@nz-holistic/nz-forms';
import { InfoModule } from '../../../info';
import { AdminInfoFieldContainerComponent } from './info-field-container.component';

@NgModule({
    imports: [CommonModule, HlcNzInputContainerModule, InfoModule],
    declarations: [AdminInfoFieldContainerComponent],
    exports: [AdminInfoFieldContainerComponent],
})
export class AdminInfoFieldContainerModule {
    constructor() {}
}
