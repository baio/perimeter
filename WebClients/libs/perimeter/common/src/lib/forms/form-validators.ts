import { FormControl } from '@angular/forms';
import {
    MISS_UPPER_CASE_LETTER,
    MISS_LOWER_CASE_LETTER,
    MISS_DIGIT,
    MISS_SPECIAL_CHAR,
    NOT_DOMAIN_NAME,
    PASSWORD,
} from './validation-errors';

const checkRegex = (regex: RegExp, value: string) =>
    !value || !value.trim() || regex.test(value as string);

const isEmpty = (value: string) => !value || /^\s+$/.test(value as string);

export class FormValidators {
    public static empty(control: FormControl) {
        const empty = isEmpty(control.value);
        return empty && { empty: true };
    }
    public static regex = (regex: RegExp, error: any) => (
        control: FormControl
    ) => {
        const isChecked = checkRegex(regex, control.value as string);
        return isChecked ? null : error;
    };

    public static password = (control: FormControl) => {
        const val = control.value as string;
        if (isEmpty(val)) {
            return PASSWORD;
        }
        const isChecked =
            checkRegex(/[A-Z]/, val) ||
            checkRegex(/[0-9]/, val) ||
            checkRegex(/[!@#$%^&*()_+=\[{\]};:<>|./?,-]/, val);
        return checkRegex(/[a-z]/, val) && isChecked ? null : PASSWORD;
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

    public static domainName = FormValidators.regex(
        /^[a-z-0-9]+$/,
        NOT_DOMAIN_NAME
    );
}
