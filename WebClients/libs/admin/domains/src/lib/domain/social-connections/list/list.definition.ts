import { AdminList } from '@admin/shared';

export const listDefinition: AdminList.TableDefinition = {
    sort: 'name',
    cols: [
        {
            id: 'name',
            title: 'Name',
            sort: true,
        },
        {
            id: 'isEnabled',
            title: 'Is Enabled',
            format: (val) => (val ? 'Yes' : 'No'),
        },
    ],
};
