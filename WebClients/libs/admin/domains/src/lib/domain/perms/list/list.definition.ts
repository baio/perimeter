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
            id: 'isDefault',
            title: 'isDefault',
            format: (f) => (f ? 'Yes' : 'No'),
        },
        {
            id: 'dateCreated',
            title: 'created',
            format: 'dateTime',
            sort: 'dateCreated',
        },
    ],
};
