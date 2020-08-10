import { HlcNzTable } from '@nz-holistic/nz-list';

export const SHOW_REFERENCES_ACTION_ID = 'SHOW_REFERENCES_ACTION_ID';

export const addDefinitionLinkButtonAction = (
    linkButtonText: string,
    definition: HlcNzTable.TableDefinition
): HlcNzTable.TableDefinition => ({
    ...definition,
    rowActions: [
        ...((definition.rowActions || []) as any),
        {
            alt: linkButtonText || 'Show References',
            iconType: 'enter',
            id: SHOW_REFERENCES_ACTION_ID,
            class: 'table-link',
        },
    ],
});
