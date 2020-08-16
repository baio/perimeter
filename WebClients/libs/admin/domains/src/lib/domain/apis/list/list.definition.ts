import { AdminList } from '@admin/shared';

export const listDefinition: AdminList.TableDefinition = {
    sort: 'dateCreated',
    cols: [
        {
            id: 'name',
            title: 'Name',
            sort: true,
        },
        {
            id: 'permissions',
            title: 'Permissions',
            kind: 'Permissions',
        },
        {
            id: 'identifier',
            title: 'Identifier',
        },
        {
            id: 'dateCreated',
            title: 'Created',
            format: 'dateTime',
            sort: 'dateCreated',
        },
    ],
    rowActions: [
        {
            id: 'permissions',
            iconType: 'lock',
        },
    ],
};
