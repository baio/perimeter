import { AdminList } from '@admin/shared';

export const listDefinition: AdminList.TableDefinition = {
    sort: {
        id: 'dateTime',
        direction: 'desc',
    },
    cols: [
        {
            id: 'userEmail',
            title: 'Email',
        },
        {
            id: 'appIdentifier',
            title: 'Application',
        },
        {
            id: 'dateTime',
            title: 'Date',
            format: 'dateTime',
            sort: true,
        },
    ],
};
