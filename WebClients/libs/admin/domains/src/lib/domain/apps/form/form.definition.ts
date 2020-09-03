import { Validators } from '@angular/forms';
import { AdminForm } from '@admin/shared';
import { Observable } from 'rxjs';

export const getDefinition = (
    isNew$: Observable<boolean>
): AdminForm.FormDefinition => ({
    kind: 'fields',
    fields: [
        {
            id: 'clientId',
            kind: 'Display',
            label: 'Client Id',
            hidden: isNew$,
        },
        {
            id: 'name',
            kind: 'Text',
            label: 'Name',
            validators: [Validators.required],
        },
        {
            id: 'idTokenExpiresIn',
            kind: 'Number',
            label: 'ID Token Expires In (minutes)',
            validators: [Validators.required],
        },
        {
            id: 'refreshTokenExpiresIn',
            kind: 'Number',
            label: 'Refresh Token Expires In (minutes)',
            validators: [Validators.required],
        },
    ],
});
