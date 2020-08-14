import { Component, OnInit } from '@angular/core';
import { listDefinition } from './list.definition';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { AdminList } from '@admin/shared';
import { AppsDataAccessService } from '@admin/data-access';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
    selector: 'admin-apps-list',
    templateUrl: './list.component.html',
    styleUrls: ['./list.component.scss'],
})
export class AppsListComponent implements OnInit {
    private readonly domainId: number;
    readonly listDefinition = listDefinition;
    readonly dataProvider: HlcNzTable.Data.DataProvider = (state) =>
        this.dataAccess.loadList(this.domainId, state);
    readonly removeItemDataAccess: AdminList.Data.RemoveItemDataAccess = ({
        id,
    }) => this.dataAccess.removeItem(this.domainId, id);

    constructor(
        activatedRoute: ActivatedRoute,
        private readonly dataAccess: AppsDataAccessService
    ) {
        this.domainId = +activatedRoute.parent.snapshot.params['id'];
    }

    ngOnInit(): void {}
}
