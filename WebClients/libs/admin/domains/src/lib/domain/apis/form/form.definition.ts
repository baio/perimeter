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
                    infoKey: 'api:name',
                },
            },
        },
        {
            id: 'identifier',
            kind: 'Text',
            hidden: isNew$.pipe(not$),
            label: 'identifier',
            validators: [Validators.required, FormValidators.domainName],
            wrapper: {
                kind: 'info',
                props: {
                    infoKey: 'api:identifier',
                },
            },
        },
        {
            id: 'identifierUri',
            kind: 'Display',
            hidden: isNew$,
            label: 'identifierUri',
            wrapper: {
                kind: 'info',
                props: {
                    infoKey: 'api:identifierUri',
                },
            },
        },
    ],
});
