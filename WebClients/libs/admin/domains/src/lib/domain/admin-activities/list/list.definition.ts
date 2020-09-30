import { AdminList } from '@admin/shared';

export const listDefinition: AdminList.TableDefinition = {
    sort: {
        id: 'dateTime',
        direction: 'desc',
    },
    cols: [
        {
            id: 'email',
            title: 'Email',
        },
        {
            id: 'dateTime',
            title: 'Date',
            format: 'dateTime',
            sort: true,
        },
    ],
};
