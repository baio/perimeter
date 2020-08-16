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
            id: 'description',
            title: 'Description',
        },
        {
            id: 'dateCreated',
            title: 'Created',
            format: 'dateTime',
            sort: 'dateCreated',
        },
    ],
};
