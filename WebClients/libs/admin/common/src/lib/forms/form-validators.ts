import { FormControl } from '@angular/forms';

export class FormValidators {
    public static empty(control: FormControl) {
        const isEmpty = !control.value || /^\s+$/.test(control.value as string);
        return isEmpty && { empty: true };
    }
}
