import { AdminList } from '@admin/shared';

export const listDefinition: AdminList.TableDefinition = {
    sort: {
        id: 'email',
        direction: 'asc',
    },
    cols: [
        {
            id: 'email',
            title: 'Email',
            sort: true,
        },
        {
            id: 'roles',
            title: 'Roles',
            kind: 'Permissions',
        },
    ],
};
