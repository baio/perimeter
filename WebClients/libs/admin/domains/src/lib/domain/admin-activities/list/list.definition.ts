import { AdminList } from '@admin/shared';
import { TranslocoService } from '@ngneat/transloco';

export const listDefinition = (
    t: (x: string) => string
): AdminList.TableDefinition => ({
    sort: {
        id: 'dateTime',
        direction: 'desc',
    },
    cols: [
        {
            id: 'userEmail',
            title: t('email'),
        },
        {
            id: 'dateTime',
            title: t('Date'),
            format: 'dateTime',
            sort: true,
        },
    ],
});
