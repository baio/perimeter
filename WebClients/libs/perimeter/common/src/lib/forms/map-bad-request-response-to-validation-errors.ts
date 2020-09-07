import { FormGroup } from '@angular/forms';
import { mergeAll } from 'lodash/fp';
import {
    EMPTY_STRING,
    MAX_LENGTH,
    MESSAGE,
    MIN_LENGTH,
    MISS_DIGIT,
    MISS_LOWER_CASE_LETTER,
    MISS_SPECIAL_CHAR,
    MISS_UPPER_CASE_LETTER,
    NOT_DOMAIN_NAME,
    PASSWORD,
} from './validation-errors';

const mapServerError = (err: string) => {
    const errs = err.split(':');
    switch (errs[0]) {
        case 'EMPTY_STRING':
            return EMPTY_STRING;
        case 'MAX_LENGTH':
            return MAX_LENGTH(+errs[1]);
        case 'MIN_LENGTH':
            return MIN_LENGTH(+errs[1]);
        case 'MESSAGE':
            return MESSAGE(errs[1]);
        case 'MISS_UPPER_LETTER':
            return MISS_UPPER_CASE_LETTER;
        case 'MISS_LOWER_LETTER':
            return MISS_LOWER_CASE_LETTER;
        case 'MISS_DIGIT':
            return MISS_DIGIT;
        case 'MISS_SPECIAL_CHAR':
            return MISS_SPECIAL_CHAR;
        case 'NOT_DOMAIN_NAME':
            return NOT_DOMAIN_NAME;
        case 'PASSWORD':
            return PASSWORD;    
        default:
            console.warn('Unmapped server bad request error content !', err);
            return null;
    }
};

export const mapBadRequestResponseToFormValidationErrors = (
    form: FormGroup,
    responseBody: any
) => {
    Object.keys(form.value).forEach((key) => {
        const errs: string[] = responseBody.data && responseBody.data[key];
        if (errs && errs.length) {
            const verrors = errs.map(mapServerError).filter((f) => !!f);
            const mverrors = mergeAll(verrors);
            form.controls[key].updateValueAndValidity();
            form.controls[key].setErrors(mverrors);
        }
    });
    // Special case, general error for whole form
    const formErrors: string[] = responseBody['__'];
    if (formErrors && formErrors.length) {
        form.setErrors({ server: formErrors });
    }
};
