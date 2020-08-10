import { HlcFormFieldWrapperMap } from '@nz-holistic/forms';
import {
    HlcNzInputContainerComponent,
    HlcNzInputContainerModule,
} from '@nz-holistic/nz-forms';
import { AdminLengthCounterContainerComponent } from '../field-container/length-counter-field-container/length-counter-field-container.component';
import { AdminLengthCounterFieldContainerModule } from '../field-container/length-counter-field-container/length-counter-field-container.module';

export const bazaFieldContainersMap: HlcFormFieldWrapperMap = {
    default: HlcNzInputContainerComponent,
    lengthCounter: AdminLengthCounterContainerComponent,
};

export const bazaFieldContainersComponents = [
    HlcNzInputContainerComponent,
    AdminLengthCounterContainerComponent,
];

export const bazaFieldContainersModules = [
    HlcNzInputContainerModule,
    AdminLengthCounterFieldContainerModule,
];
