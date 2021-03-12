import { HlcNzTable } from '@nz-holistic/nz-list';

export const addDefinitionTranslations = (
    t: (s: string) => string,
    definition: HlcNzTable.TableDefinition
): HlcNzTable.TableDefinition => ({
    ...definition,
    cols: definition.cols.map((col) => ({ ...col, title: t(col.title) })),
});
