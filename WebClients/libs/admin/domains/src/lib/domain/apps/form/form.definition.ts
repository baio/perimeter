import { Validators } from '@angular/forms';
import { AdminForm } from '@admin/shared';

export const definition: AdminForm.FormDefinition = {
    kind: 'fields',
    fields: [
        {
            id: 'clientId',
            kind: 'Display',
            label: 'Client Id',
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
};
