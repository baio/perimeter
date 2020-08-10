import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormControl } from '@angular/forms';
import { HlcNzFormFields, HlcNzInputContainer } from '@nz-holistic/nz-forms';
import { isNil } from 'lodash/fp';

@Component({
    selector: 'admin-length-counter-field-container',
    templateUrl: './length-counter-field-container.component.html',
    styleUrls: ['./length-counter-field-container.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminLengthCounterContainerComponent implements HlcNzInputContainer {
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

    @Input() maxLength: number;
    @Input() warnLength: number;

    constructor() {}


    get counterClass() {
        if (isNil(this.leftCounter)) {
            return null;
        }
        else if (this.leftCounter < 0) {
            return 'text-counter-danger';
        }
        else if (!isNil(this.warnLength) && !isNil(this.maxLength) && this.warnLength < this.currentLength) {
            return 'text-counter-warning';
        }
    }

    get currentLength() {
        return isNil(this.value) ? 0 : this.value.length;
    }

    get leftCounter() {
        return isNil(this.maxLength) ? null : this.maxLength - this.currentLength;
    }

    get value() {
        return this.formControl && this.formControl.value;
    }
}
