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
            label: 'Name',
            validators: [Validators.required],
        },
        {
            id: 'signingAlgorithm',
            kind: 'Display',
            label: 'Signing Algorithm',
            hidden: isNew$,
        },
        {
            id: 'signingSecret',
            kind: 'Display',
            label: 'Signing Secret',
            hidden: isNew$,
        },
        {
            id: 'identifier',
            kind: 'Text',
            hidden: isNew$.pipe(not$),
            label: 'Identifier',
            validators: [Validators.required, FormValidators.domainName],
        },
        {
            id: 'identifierUri',
            kind: 'Display',
            hidden: isNew$,
            label: 'Identifier',
        },
        {
            id: 'accessTokenExpiresIn',
            kind: 'Number',
            label: 'Access Token Expires in Minutes',
            validators: [Validators.required],
        },
    ],
});
