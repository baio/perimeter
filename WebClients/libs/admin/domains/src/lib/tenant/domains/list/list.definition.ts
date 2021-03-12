import { AdminList } from '@admin/shared';

export const listDefinition: AdminList.TableDefinition = {
    sort: 'dateCreated',
    cols: [
        {
            id: 'name',
            title: 'name',
            decorator: 'ellipsis',
            sort: true,
        },
        {
            id: 'identifier',
            title: 'identifier',
            decorator: 'ellipsis',
            sort: true,
        },
        {
            id: 'envs',
            title: 'environments',
            customCell: true,
        },
        {
            id: 'dateCreated',
            title: 'created',
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
