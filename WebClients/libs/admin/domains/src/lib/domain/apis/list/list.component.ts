import { ApisDataAccessService } from '@admin/data-access';
import { AdminList } from '@admin/shared';
import { Component, OnInit } from '@angular/core';
import { HlcNzTable, ActionClickEvent } from '@nz-holistic/nz-list';
import { listDefinition } from './list.definition';
import { ActivatedRoute, Route, Router } from '@angular/router';

@Component({
    selector: 'admin-apis-list',
    templateUrl: './list.component.html',
    styleUrls: ['./list.component.scss'],
})
export class ApisListComponent implements OnInit {
    private readonly domainId: number;
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

    onActionClick($event: ActionClickEvent) {
        if ($event.actionId === 'permissions') {
            this.router.navigate(['.', $event.row.id, 'permissions'], {
                relativeTo: this.activatedRoute,
            });
        }
    }
}
