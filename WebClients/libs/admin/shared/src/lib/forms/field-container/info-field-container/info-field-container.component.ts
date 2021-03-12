import {
    ChangeDetectionStrategy,
    Component,
    Input,
    OnChanges,
} from '@angular/core';
import { FormControl } from '@angular/forms';
import { HlcNzFormFields, HlcNzInputContainer } from '@nz-holistic/nz-forms';
import { isNil } from 'lodash/fp';

@Component({
    selector: 'admin-info-field-container',
    templateUrl: './info-field-container.component.html',
    styleUrls: ['./info-field-container.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminInfoFieldContainerComponent implements HlcNzInputContainer {
    @Input()
    label: string;

    @Input()
    id: string;

    @Input()
    validatorsErrorsMap: HlcNzFormFields.FieldValidatorsErrorsMap | undefined;

    @Input()
    readonly: boolean;

    @Input()
    formControl: FormControl;

    @Input() infoKey: string;

    constructor() {}
}
