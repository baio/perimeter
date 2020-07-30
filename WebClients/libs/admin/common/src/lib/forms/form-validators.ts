import { FormControl } from '@angular/forms';

export class FormValidators {
    public static empty(control: FormControl) {
        const isEmpty = !control.value || /^\s+$/.test(control.value as string);
        return isEmpty && { empty: true };
    }
    public static regex = (regex: RegExp, error: any) => (
        control: FormControl
    ) => {
        const isChecked =
            !control.value ||
            !control.value.trim() ||
            regex.test(control.value as string);
        return isChecked ? null : error;
    };
}
