import { Validators } from '@angular/forms';
import { AdminForm } from '@admin/shared';
import { FormValidators } from '@perimeter/common';

export const definition: AdminForm.FormDefinition = {
    kind: 'fields',
    fields: [
        {
            id: 'name',
            kind: 'Text',
            label: 'Name',
            validators: [Validators.required, FormValidators.domainName],
        },
    ],
};
