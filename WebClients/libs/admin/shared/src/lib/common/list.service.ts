import { Injectable } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { Subject } from 'rxjs';

@Injectable()
export class AdminListService {
    private readonly _rowAdded = new Subject<HlcNzTable.Row>();
    private readonly _rowUpdated = new Subject<HlcNzTable.Row>();
    private readonly _rowRemoved = new Subject<HlcNzTable.Row>();

    get rowUpdated() {
        return this._rowUpdated.asObservable();
    }

    get rowAdded() {
        return this._rowAdded.asObservable();
    }

    get rowRemoved() {
        return this._rowRemoved.asObservable();
    }

    onRowUpdated(row: HlcNzTable.Row) {
        this._rowUpdated.next(row);
    }

    onRowAdded(row: HlcNzTable.Row) {
        this._rowAdded.next(row);
    }

    onRowRemoved(row: HlcNzTable.Row) {
        this._rowRemoved.next(row);
    }

}
