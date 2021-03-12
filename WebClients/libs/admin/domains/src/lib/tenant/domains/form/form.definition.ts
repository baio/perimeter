import { Validators } from '@angular/forms';
import { AdminForm, not$ } from '@admin/shared';
import { Observable } from 'rxjs';
import { FormValidators } from '@perimeter/common';

export const getDefinition = (
    isNew$: Observable<boolean>
): AdminForm.FormDefinition => ({
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
                    infoKey: 'tenant:name',
                },
            },
        },
        {
            id: 'identifier',
            kind: 'Text',
            props: {
                label: 'identifier',
                readonly: isNew$.pipe(not$),
            },
            validators: [Validators.required, FormValidators.domainName],
            wrapper: {
                kind: 'info',
                props: {
                    infoKey: 'tenant:identifier',
                },
            },
        },
    ],
});
