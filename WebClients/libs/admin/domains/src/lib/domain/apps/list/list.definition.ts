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
            id: 'clientId',
            title: 'Client Id',
        },
        {
            id: 'ssoEnabled',
            title: 'Use SSO',
            format: (val) => (val ? 'Yes' : 'No'),
        },
        {
            id: 'allowedCallbackUrls',
            title: 'Return URIs',
        },
        {
            id: 'dateCreated',
            title: 'Created',
            format: 'dateTime',
            sort: 'dateCreated',
        },
    ],
};
