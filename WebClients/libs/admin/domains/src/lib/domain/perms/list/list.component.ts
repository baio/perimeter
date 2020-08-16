import {
    ApisDataAccessService,
    PermissionsDataAccessService,
} from '@admin/data-access';
import { AdminList } from '@admin/shared';
import { Component, OnInit } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { listDefinition } from './list.definition';
import { ActivatedRoute } from '@angular/router';

@Component({
    selector: 'admin-perms-list',
    templateUrl: './list.component.html',
    styleUrls: ['./list.component.scss'],
})
export class PermsListComponent implements OnInit {
    private readonly apiId: number;
    readonly listDefinition = listDefinition;
    readonly dataProvider: HlcNzTable.Data.DataProvider = (state) =>
        this.dataAccess.loadList(this.apiId, state);
    readonly removeItemDataAccess: AdminList.Data.RemoveItemDataAccess = ({
        id,
    }) => this.dataAccess.removeItem(this.apiId, id);

    constructor(
        activatedRoute: ActivatedRoute,
        private readonly dataAccess: PermissionsDataAccessService
    ) {
        this.apiId = +activatedRoute.parent.snapshot.params['id'];
    }

    ngOnInit(): void {}
}
