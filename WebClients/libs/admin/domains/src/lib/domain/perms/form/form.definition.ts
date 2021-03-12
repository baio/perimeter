import { AdminForm } from '@admin/shared';
import { Validators } from '@angular/forms';

export const definition: AdminForm.FormDefinition = {
    kind: 'fields',
    fields: [
        {
            id: 'name',
            kind: 'Text',
            label: 'name',
            validators: [Validators.required],
            wrapper: {
                kind: 'info',
                props: {
                    infoKey: 'perms:name',
                },
            },
        },
        {
            id: 'description',
            kind: 'TextArea',
            label: 'description',
            validators: [Validators.required],
            wrapper: {
                kind: 'info',
                props: {
                    infoKey: 'perms:description',
                },
            },
        },
        {
            id: 'isDefault',
            kind: 'Toggle',
            label: 'isDefault',
            props: {
                value: false,
            },
            validators: [Validators.required],
            wrapper: {
                kind: 'info',
                props: {
                    infoKey: 'perms:is-default',
                },
            },
        },
    ],
};
