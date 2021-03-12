import { AdminList } from '@admin/shared';

export const listDefinition: AdminList.TableDefinition = {
    sort: {
        id: 'email',
        direction: 'asc',
    },
    cols: [
        {
            id: 'email',
            title: 'email',
            sort: true,
        },
        {
            id: 'roles',
            title: 'roles',
            // TODO : Rename
            kind: 'Permissions',
        },
    ],
};
