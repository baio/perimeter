import { AdminList } from '@admin/shared';

export const listDefinition: AdminList.TableDefinition = {
    sort: 'dateCreated',
    cols: [
        {
            id: 'name',
            title: 'Name'
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
};