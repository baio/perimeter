import { HlcNzTable } from '@nz-holistic/nz-list';
import { Observable, Subject } from 'rxjs';

export namespace AdminList {
    export namespace Columns {
        export interface AdminPermissionsColumn
            extends HlcNzTable.MapColumns.MapColumn<{
                permissions: string[];
                hasEditButton?: boolean;
                clicked?: Subject<MouseEvent>;
            }> {
            kind: 'Permissions';
        }

        export type CustomMapColumns = AdminPermissionsColumn;
    }

    export interface TableDefinition
        extends HlcNzTable.TableDefinition<
            Columns.CustomMapColumns | HlcNzTable.MapColumns.Column
        > {
        hasLinkButton?: string | boolean;
    }

    export namespace Data {
        export type RemoveItemDataAccess = (
            row: HlcNzTable.Row
        ) => Observable<any>;
    }

    export type CheckRowFun<T extends HlcNzTable.Row = HlcNzTable.Row> = (row: T) => boolean;    
}
