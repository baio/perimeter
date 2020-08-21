import {
    ApisDataAccessService,
    AdminsDataAccessService,
    TenantAdminsDataAccessService,
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
    readonly listDefinition = listDefinition;
    readonly dataProvider: HlcNzTable.Data.DataProvider = (state) =>
        this.dataAccess.loadList(state);
    readonly removeItemDataAccess: AdminList.Data.RemoveItemDataAccess = ({
        id,
    }) => {
        return this.dataAccess.removeItem(id);
    };

    constructor(
        activatedRoute: ActivatedRoute,
        private readonly dataAccess: TenantAdminsDataAccessService
    ) {}

    ngOnInit(): void {}
}