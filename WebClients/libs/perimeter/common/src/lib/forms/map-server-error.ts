import { FormGroup } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { mapBadRequestResponseToFormValidationErrors } from './map-bad-request-response-to-validation-errors';
import { mapConflictResponseToFormValidationErrors } from './map-conflict-response-to-validation-errors';

export const mapServerError = (form: FormGroup, _err: any) => {
    const err = _err as HttpErrorResponse;
    if (err.status === 400) {
        mapBadRequestResponseToFormValidationErrors(form, err.error);
        return 'Some fields have invalid data';
    } else if (err.status === 409) {
        mapConflictResponseToFormValidationErrors(form, err.error);
        return 'Some fields have conflicted data';
    } else {
        return err.message;
    }
};
