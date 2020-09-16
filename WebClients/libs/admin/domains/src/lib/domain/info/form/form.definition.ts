import { AdminForm } from '@admin/shared';
import { Validators } from '@angular/forms';

export const definition: AdminForm.FormDefinition = {
    kind: 'fields',
    fields: [
        {
            id: 'envName',
            kind: 'Text',
            label: 'Name',
            validators: [Validators.required],
        },
        /*
        {
            id: 'signingAlgorithm',
            kind: 'Display',
            label: 'Signing Algorithm',
            hidden: isNew$,
        },
        {
            id: 'signingSecret',
            kind: 'Display',
            label: 'Signing Secret',
            hidden: isNew$,
        },*/
        {
            id: 'issuer',
            kind: 'Display',
            label: 'Issuer',
        },
        /*
        {
            id: 'accessTokenExpiresIn',
            kind: 'Number',
            label: 'Access Token Expires in Minutes',
            validators: [Validators.required],
        },*/
    ],
};
