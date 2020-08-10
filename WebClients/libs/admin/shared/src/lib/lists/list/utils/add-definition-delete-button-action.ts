import { HlcNzTable } from '@nz-holistic/nz-list';

export const DELETE_ACTION_ID = 'DELETE_ACTION_ID';

export const addDefinitionDeleteButtonAction = (
    definition: HlcNzTable.TableDefinition
): HlcNzTable.TableDefinition => ({
    ...definition,
    rowActions: (row) => {
        const rowActions = typeof definition.rowActions === 'function' ? definition.rowActions(row) : definition.rowActions || [];

        return [
            {
                alt: 'Delete',
                id: DELETE_ACTION_ID,
                iconType: 'delete',
                class: 'table-delete',
            },
            ...rowActions,
        ];
    },
});
