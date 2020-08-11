import { Validators } from '@angular/forms';
import { AdminForm } from '@admin/shared';

export const definition: AdminForm.FormDefinition = {
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
            label: 'Identifier',
            validators: [Validators.required],
        },
    ],
};
