import { Validators } from '@angular/forms';
import { AdminForm, not$ } from '@admin/shared';
import { Observable } from 'rxjs';

export const getDefinition = (
    isNew$: Observable<boolean>
): AdminForm.FormDefinition => ({
    kind: 'fields',
    fields: [
        {
            id: 'name',
            kind: 'Text',
            label: 'Name',
            validators: [Validators.required],
        },
        {
            id: 'identifier',
            kind: 'Text',
            props: {
                readonly: isNew$.pipe(not$),
                label: 'Identifier',
            },
            validators: [Validators.required],
        },
        {
            id: 'accessTokenExpiresIn',
            kind: 'Number',
            label: 'Access Token Expires in Minutes',
            validators: [Validators.required],
        },
    ],
});
