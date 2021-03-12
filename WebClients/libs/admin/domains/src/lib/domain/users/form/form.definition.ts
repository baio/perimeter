import { Validators } from '@angular/forms';
import { AdminForm, not$ } from '@admin/shared';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { map as _map } from 'lodash/fp';

export const getDefinition = (
    isNew$: Observable<boolean>,
    roles$: Observable<any[]>
): AdminForm.FormDefinition => ({
    kind: 'fields',
    fields: [
        {
            id: 'userEmail',
            kind: 'Text',
            label: 'Email',
            validators: [Validators.required, Validators.email],
            props: {
                readonly: isNew$.pipe(not$),
            },
            wrapper: {
                kind: 'info',
                props: {
                    infoKey: 'users:email',
                },
            },
        },
        {
            id: 'rolesIds',
            kind: 'Select',
            props: {
                label: 'Roles',
                mode: 'tags',
                items: roles$.pipe(
                    map(_map((x) => ({ key: x.id, label: x.name })))
                ),
            },
            validators: [Validators.required],
            wrapper: {
                kind: 'info',
                props: {
                    infoKey: 'users:roles',
                },
            },
        },
    ],
});
