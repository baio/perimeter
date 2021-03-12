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
            id: 'appIdentifier',
            title: 'application',
        },
        {
            id: 'dateTime',
            title: 'date',
            format: 'dateTime',
            sort: true,
        },
    ],
};
