import { HlcNzTable } from '@nz-holistic/nz-list';
import { Observable } from 'rxjs';

export namespace AdminList {
    /*
    export namespace Columns {
        export interface AdminStateColumn extends HlcNzTable.MapColumns.MapColumn<{ state: AdminSate }> {
            kind: 'AdminState';
        }

        export type CustomMapColumns = AdminStateColumn;
    }
    */

    export interface TableDefinition
        extends HlcNzTable.TableDefinition<
            /*Columns.CustomMapColumns |*/ HlcNzTable.MapColumns.Column
        > {
        hasLinkButton?: string | boolean;
    }

    export namespace Data {
        export type RemoveItemDataAccess = (
            row: HlcNzTable.Row
        ) => Observable<any>;
    }
}
