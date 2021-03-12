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
                label: 'name',
            },
            {
                id: 'isEnabled',
                kind: 'Toggle',
                label: 'isEnabled',
            },
            {
                id: 'clientId',
                kind: 'Text',
                label: 'clientID',
                validators: [Validators.required],
                hidden: isNotEnabled$,
            },
            {
                id: 'clientSecret',
                kind: 'Text',
                label: 'clientSecret',
                validators: [Validators.required],
                hidden: isNotEnabled$,
            },
        ],
    };
};
