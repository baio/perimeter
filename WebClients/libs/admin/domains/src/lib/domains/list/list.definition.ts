import { AdminList } from '@admin/shared';

export const listDefinition: AdminList.TableDefinition = {
    sort: 'created',
    cols: [
        {
            id: 'name',
            title: 'Name',
            decorator: 'ellipsis',
        },
        {
            id: 'envs',
            title: 'Environments',
            customCell: true,
        },
        {
            id: 'created',
            title: 'Created',
            format: 'dateTime',
            sort: 'dateCreated',
        },
    ],
};
