import { HlcFormFieldWrapperMap } from '@nz-holistic/forms';
import {
    HlcNzInputContainerComponent,
    HlcNzInputContainerModule,
} from '@nz-holistic/nz-forms';
import { AdminLengthCounterContainerComponent } from '../field-container/length-counter-field-container/length-counter-field-container.component';
import { AdminLengthCounterFieldContainerModule } from '../field-container/length-counter-field-container/length-counter-field-container.module';

export const adminFieldContainersMap: HlcFormFieldWrapperMap = {
    default: HlcNzInputContainerComponent,
    lengthCounter: AdminLengthCounterContainerComponent,
};

export const adminFieldContainersComponents = [
    HlcNzInputContainerComponent,
    AdminLengthCounterContainerComponent,
];

export const adminFieldContainersModules = [
    HlcNzInputContainerModule,
    AdminLengthCounterFieldContainerModule,
];
