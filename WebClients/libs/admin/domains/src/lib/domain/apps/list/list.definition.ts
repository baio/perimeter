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
            id: 'grantTypes',
            title: 'grantTypes',
        },
        {
            id: 'clientId',
            title: 'clientId',
        },
        {
            id: 'ssoEnabled',
            title: 'useSSO',
            format: (val) => (val ? 'Yes' : 'No'),
        },
        {
            id: 'allowedCallbackUrls',
            title: 'returnUris',
        },
        {
            id: 'dateCreated',
            title: 'created',
            format: 'dateTime',
            sort: 'dateCreated',
        },
    ],
};
