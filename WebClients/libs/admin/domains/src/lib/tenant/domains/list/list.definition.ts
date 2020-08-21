import { AdminList } from '@admin/shared';

export const listDefinition: AdminList.TableDefinition = {
    sort: 'dateCreated',
    cols: [
        {
            id: 'name',
            title: 'Name',
            decorator: 'ellipsis',
            sort: true,
        },
        {
            id: 'envs',
            title: 'Environments',
            customCell: true,
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
            id: 'add-env',
            iconType: 'plus-square'
        }
    ]
};