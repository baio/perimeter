import { FormGroup } from '@angular/forms';
import { UNIQUE_CONFLICT } from './validation-errors';

const mapServerError = (err: string) => {
    switch (err) {
        case 'UNIQUE':
            return UNIQUE_CONFLICT;
        default:
            console.warn('Unmapped server conflict request error content !', err);
            return null;
    }
};

export const mapConflictResponseToFormValidationErrors = (
    form: FormGroup,
    { field, code }: { field: string; code: string }
) => {
    const err = mapServerError(code);
    if (err) {
        form.controls[field].updateValueAndValidity();
        form.controls[field].setErrors(err);        
    }
};
