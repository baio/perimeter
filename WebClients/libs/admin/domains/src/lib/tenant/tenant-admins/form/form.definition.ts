import { Validators } from '@angular/forms';
import { AdminForm } from '@admin/shared';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { map as _map } from 'lodash/fp';

export const getDefinition = (
    roles$: Observable<any[]>
): AdminForm.FormDefinition => ({
    kind: 'fields',
    fields: [
        {
            id: 'userEmail',
            kind: 'Text',
            label: 'Name',
            validators: [Validators.required],
        },
        {
            id: 'roleId',
            kind: 'Select',
            props: {
                label: 'Role',
                mode: 'default',
                items: roles$.pipe(
                    map(_map((x) => ({ key: x.id, label: x.name })))
                ),
            },
            validators: [Validators.required],
        },
    ],
});
