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
            id: 'permissions',
            title: 'permissions',
            kind: 'Permissions',
            // TODO !
            props: {
                hasEditButton: true,
            } as any,
        },
        {
            id: 'identifierUri',
            title: 'identifier',
        }/*,
        {
            id: 'signingAlgorithm',
            title: 'Signing Algorithm',
        },
        {
            id: 'signingSecret',
            title: 'Signing Secret',
        }*/,        
        {
            id: 'dateCreated',
            title: 'created',
            format: 'dateTime',
            sort: 'dateCreated',
        },
    ],
};
