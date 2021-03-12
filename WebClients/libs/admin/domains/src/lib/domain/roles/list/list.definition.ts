import { AdminList } from '@admin/shared';

export const listDefinition: AdminList.TableDefinition = {
    sort: 'dateCreated',
    cols: [
        {
            id: 'name',
            title: 'name',
            sort: true,
        },
        {
            id: 'description',
            title: 'description',
        },
        {
            id: 'permissions',
            title: 'permissions',
            kind: 'Permissions',
        },
        {
            id: 'dateCreated',
            title: 'created',
            format: 'dateTime',
            sort: 'dateCreated',
        },
    ],
};
