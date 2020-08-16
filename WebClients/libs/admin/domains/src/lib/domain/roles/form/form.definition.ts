import { Validators } from '@angular/forms';
import { AdminForm } from '@admin/shared';
import { Observable } from 'rxjs';
import { Permission } from '@admin/data-access';
import { map } from 'rxjs/operators';
import { map as _map } from 'lodash/fp';

export const getDefinition = (
    permissions$: Observable<Permission[]>
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
            id: 'description',
            kind: 'TextArea',
            label: 'Description',
            validators: [Validators.required],
        },
        {
            id: 'permissionIds',
            kind: 'Select',
            props: {
                label: 'Permissions',
                mode: 'tags',
                items: permissions$.pipe(
                    map(_map((x) => ({ key: x.id, label: x.name })))
                ),
            },
        },
    ],
});
