import { AdminList } from '@admin/shared';

export const listDefinition: AdminList.TableDefinition = {
    sort: {
        id: 'dateTime',
        direction: 'desc',
    },
    cols: [
        {
            id: 'userEmail',
            title: 'email',
        },
        {
            id: 'dateTime',
            title: 'date',
            format: 'dateTime',
            sort: true,
        },
    ],
};
