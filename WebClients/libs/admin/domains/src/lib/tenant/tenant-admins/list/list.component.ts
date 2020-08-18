import {
    ApisDataAccessService,
    AdminsDataAccessService,
} from '@admin/data-access';
import { AdminList } from '@admin/shared';
import { Component, OnInit } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { listDefinition } from './list.definition';
import { ActivatedRoute } from '@angular/router';

@Component({
    selector: 'admin-tenant-admins-list',
    templateUrl: './list.component.html',
    styleUrls: ['./list.component.scss'],
})
export class TenantAdminsListComponent implements OnInit {
    private readonly domainId: number;
    readonly listDefinition = listDefinition;
    readonly dataProvider: HlcNzTable.Data.DataProvider = (state) =>
        this.dataAccess.loadList(this.domainId, state);
    readonly removeItemDataAccess: AdminList.Data.RemoveItemDataAccess = ({
        id,
    }) => {
        return this.dataAccess.removeItem(this.domainId, id);
    };

    constructor(
        activatedRoute: ActivatedRoute,
        private readonly dataAccess: AdminsDataAccessService
    ) {
        this.domainId = +activatedRoute.parent.snapshot.params['id'];
    }

    ngOnInit(): void {}
}
