import { FormControl } from '@angular/forms';
import {
    MISS_UPPER_CASE_LETTER,
    MISS_LOWER_CASE_LETTER,
    MISS_DIGIT,
    MISS_SPECIAL_CHAR,
    NOT_DOMAIN_NAME,
} from './validation-errors';

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

    // tslint:disable: member-ordering
    public static missUpperCaseLetter = FormValidators.regex(
        /[A-Z]/,
        MISS_UPPER_CASE_LETTER
    );
    public static missLowerCaseLetter = FormValidators.regex(
        /[a-z]/,
        MISS_LOWER_CASE_LETTER
    );
    public static missDigit = FormValidators.regex(/[0-9]/, MISS_DIGIT);
    public static missSpecialChar = FormValidators.regex(
        /[!@#$%^&*()_+=\[{\]};:<>|./?,-]/,
        MISS_SPECIAL_CHAR
    );

    public static notDomainName = FormValidators.regex(
        /^[a-z-0-9]+$/,
        NOT_DOMAIN_NAME
    );
}
