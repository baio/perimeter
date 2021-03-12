import { HlcFormFieldWrapperMap } from '@nz-holistic/forms';
import {
    HlcNzInputContainerComponent,
    HlcNzInputContainerModule,
} from '@nz-holistic/nz-forms';
import { AdminInfoFieldContainerComponent } from '../field-container/info-field-container/info-field-container.component';
import { AdminInfoFieldContainerModule } from '../field-container/info-field-container/info-field-container.module';
import { AdminLengthCounterContainerComponent } from '../field-container/length-counter-field-container/length-counter-field-container.component';
import { AdminLengthCounterFieldContainerModule } from '../field-container/length-counter-field-container/length-counter-field-container.module';

export const adminFieldContainersMap: HlcFormFieldWrapperMap = {
    default: HlcNzInputContainerComponent,
    lengthCounter: AdminLengthCounterContainerComponent,
    info: AdminInfoFieldContainerComponent,
};

export const adminFieldContainersComponents = [
    HlcNzInputContainerComponent,
    AdminLengthCounterContainerComponent,
    AdminInfoFieldContainerComponent,
];

export const adminFieldContainersModules = [
    HlcNzInputContainerModule,
    AdminLengthCounterFieldContainerModule,
    AdminInfoFieldContainerModule,
];
