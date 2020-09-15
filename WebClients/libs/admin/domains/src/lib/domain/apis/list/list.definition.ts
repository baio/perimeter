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
            id: 'permissions',
            title: 'Permissions',
            kind: 'Permissions',
            // TODO !
            props: {
                hasEditButton: true,
            } as any,
        },
        {
            id: 'identifierUri',
            title: 'Identifier',
        },
        {
            id: 'signingAlgorithm',
            title: 'Signing Algorithm',
        },
        {
            id: 'signingSecret',
            title: 'Signing Secret',
        },
        {
            id: 'dateCreated',
            title: 'Created',
            format: 'dateTime',
            sort: 'dateCreated',
        },
    ],
};
