import { AdminList } from '@admin/shared';

export const listDefinition: AdminList.TableDefinition = {
    sort: 'name',
    cols: [
        {
            id: 'name',
            title: 'name',
            sort: true,
        },
        {
            id: 'isEnabled',
            title: 'isEnabled',
            sort: true,
            format: (val) => (val ? 'Yes' : 'No'),
        },
    ],
};
