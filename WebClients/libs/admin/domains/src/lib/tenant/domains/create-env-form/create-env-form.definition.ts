import { Validators } from '@angular/forms';
import { AdminForm } from '@admin/shared';

export const definition: AdminForm.FormDefinition = {
    kind: 'fields',
    fields: [
        {
            id: 'envName',
            kind: 'Text',
            label: 'name',
            validators: [Validators.required],
        },
    ],
};
