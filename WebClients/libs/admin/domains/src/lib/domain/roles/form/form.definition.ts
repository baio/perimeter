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
            id: 'description',
            kind: 'TextArea',
            label: 'Description',
            validators: [Validators.required],
        },
    ],
};
