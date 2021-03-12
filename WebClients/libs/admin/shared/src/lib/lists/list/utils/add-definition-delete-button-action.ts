import { HlcNzTable } from '@nz-holistic/nz-list';
import { AdminList } from '../list.models';

export const DELETE_ACTION_ID = 'DELETE_ACTION_ID';

export const addDefinitionDeleteButtonAction = (
    t: (s: string) => string,
    definition: HlcNzTable.TableDefinition,
    checkRow?: AdminList.CheckRowFun
): HlcNzTable.TableDefinition => ({
    ...definition,
    rowActions: (row) => {
        const rowActions =
            typeof definition.rowActions === 'function'
                ? definition.rowActions(row)
                : definition.rowActions || [];

        const deleteRowActions =
            checkRow && !checkRow(row)
                ? []
                : [
                      {
                          alt: t('delete'),
                          id: DELETE_ACTION_ID,
                          iconType: 'delete',
                          class: 'table-delete',
                      },
                  ];

        return [...rowActions, ...deleteRowActions];
    },
});
