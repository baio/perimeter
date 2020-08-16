import { AdminList } from '@admin/shared';

export const listDefinition: AdminList.TableDefinition = {
    sort: 'email',
    cols: [
        {
            id: 'email',
            title: 'Email',
            sort: true,
        },
        {
            id: 'roles',
            title: 'Roles',
            // TODO : Rename
            kind: 'Permissions'
        },
    ],
};
