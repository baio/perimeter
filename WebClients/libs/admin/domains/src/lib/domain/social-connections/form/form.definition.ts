import { AdminForm } from '@admin/shared';
import { Validators } from '@angular/forms';

export const definition: AdminForm.FormDefinition = {
    kind: 'fields',
    fields: [
        {
            id: 'name',
            kind: 'Text',
            label: 'Name',
            props: {
                readonly: true,
            },
        },
        {
            id: 'isEnabled',
            kind: 'Text',
            label: 'Is Enabled',
        },
        {
            id: 'clientId',
            kind: 'Text',
            label: 'Client ID',
            validators: [Validators.required],
        },
        {
            id: 'clientSecret',
            kind: 'Text',
            label: 'Client Secret',
            validators: [Validators.required],
        },
        {
            id: 'attributes',
            kind: 'Select',
            props: {
                label: 'Attributes',
                mode: 'tags',
                items: [],
            },
        },
        {
            id: 'permissions',
            kind: 'Select',
            props: {
                label: 'Permissions',
                mode: 'tags',
                items: [],
            },
        },
    ],
};
