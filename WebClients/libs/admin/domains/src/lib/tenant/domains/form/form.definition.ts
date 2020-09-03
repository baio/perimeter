import { Validators } from '@angular/forms';
import { AdminForm, not$ } from '@admin/shared';
import { Observable } from 'rxjs';

export const getDefinition = (
    isNew$: Observable<boolean>
): AdminForm.FormDefinition => ({
    kind: 'fields',
    fields: [
        {
            id: 'name',
            kind: 'Text',
            props: {
                label: 'Name',
                readonly: isNew$.pipe(not$),
            },
            validators: [Validators.required],
        },
    ],
});
