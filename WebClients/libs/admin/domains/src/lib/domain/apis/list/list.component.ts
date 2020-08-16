import { ApisDataAccessService } from '@admin/data-access';
import { AdminList } from '@admin/shared';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CellClickEvent, HlcNzTable } from '@nz-holistic/nz-list';
import { Subject } from 'rxjs';
import { listDefinition } from './list.definition';

@Component({
    selector: 'admin-apis-list',
    templateUrl: './list.component.html',
    styleUrls: ['./list.component.scss'],
})
export class ApisListComponent implements OnInit, OnDestroy {
    private readonly domainId: number;
    private readonly destroy$ = new Subject();
    readonly listDefinition = listDefinition;
    readonly dataProvider: HlcNzTable.Data.DataProvider = (state) =>
        this.dataAccess.loadList(this.domainId, state);
    readonly removeItemDataAccess: AdminList.Data.RemoveItemDataAccess = ({
        id,
    }) => this.dataAccess.removeItem(this.domainId, id);

    constructor(
        private readonly router: Router,
        private readonly activatedRoute: ActivatedRoute,
        private readonly dataAccess: ApisDataAccessService
    ) {
        this.domainId = +activatedRoute.parent.snapshot.params['id'];
    }

    ngOnInit(): void {}

    ngOnDestroy(): void {
        this.destroy$.next();
    }

    onCellClick($event: CellClickEvent) {
        if ($event.col.id === 'permissions') {
            $event.$event.preventDefault();
            $event.$event.stopPropagation();
            this.router.navigate(['.', $event.row.id, 'permissions'], {
                relativeTo: this.activatedRoute,
            });
        }
    }
}
