import { AdminForm, not$ } from '@admin/shared';
import { FormGroup, Validators } from '@angular/forms';
import { propChanged } from '@nz-holistic/forms';
export const definition = (formGroup: FormGroup): AdminForm.FormDefinition => {
    const isNotEnabled$ = formGroup.valueChanges.pipe(
        propChanged('isEnabled'),
        not$
    );
    return {
        kind: 'fields',
        fields: [
            {
                id: 'name',
                kind: 'Display',
                label: 'Name',
            },
            {
                id: 'isEnabled',
                kind: 'Toggle',
                label: 'Is Enabled',
            },
            {
                id: 'clientId',
                kind: 'Text',
                label: 'Client ID',
                validators: [Validators.required],
                hidden: isNotEnabled$,
            },
            {
                id: 'clientSecret',
                kind: 'Text',
                label: 'Client Secret',
                validators: [Validators.required],
                hidden: isNotEnabled$,
            },
        ],
    };
};
