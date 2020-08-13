import { Component, OnInit } from '@angular/core';
import { listDefinition } from './list.definition';
import { HlcNzTable, ActionClickEvent } from '@nz-holistic/nz-list';
import { AdminList } from '@admin/shared';
import { DomainsDataAccessService } from '@admin/data-access';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
    selector: 'admin-domains-pool-list',
    templateUrl: './list.component.html',
    styleUrls: ['./list.component.scss'],
})
export class DomainsPoolListComponent implements OnInit {
    readonly listDefinition = listDefinition;
    readonly dataProvider: HlcNzTable.Data.DataProvider = (state) =>
        this.dataAccess.loadList(state);
    readonly removeItemDataAccess: AdminList.Data.RemoveItemDataAccess = ({
        id,
    }) => this.dataAccess.removeItem(id);

    constructor(
        private readonly dataAccess: DomainsDataAccessService,
        private readonly router: Router,
        private readonly activatedRoute: ActivatedRoute
    ) {}

    ngOnInit(): void {}

    onActionClick($event: ActionClickEvent) {
        if ($event.actionId === 'add-env') {
            this.router.navigate(['.', $event.row.id, 'new-env'], {
                relativeTo: this.activatedRoute,
            });
        }
    }
}
