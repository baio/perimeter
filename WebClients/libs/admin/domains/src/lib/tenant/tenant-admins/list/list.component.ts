import {
    ApisDataAccessService,
    AdminsDataAccessService,
    TenantAdminsDataAccessService,
    UserRole,
    USER_ROLE_TENANT_OWNER_ID,
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
    private readonly tenantId = +this.activatedRoute.parent.snapshot.params['id'];
    readonly listDefinition = listDefinition;
    readonly dataProvider: HlcNzTable.Data.DataProvider = (state) =>
        this.dataAccess.loadList(this.tenantId, state);
    readonly removeItemDataAccess: AdminList.Data.RemoveItemDataAccess = ({
        id,
    }) => {
        return this.dataAccess.removeItem(this.tenantId, id);
    };

    readonly canRemoveRow: AdminList.CheckRowFun<UserRole> = (row) =>
        row.roles.some((x) => x.id !== USER_ROLE_TENANT_OWNER_ID);

    readonly beforeRowClick: AdminList.CheckRowFun<UserRole> = (row) =>
        row.roles.some((x) => x.id !== USER_ROLE_TENANT_OWNER_ID);

    constructor(
        private readonly activatedRoute: ActivatedRoute,
        private readonly dataAccess: TenantAdminsDataAccessService
    ) {}

    ngOnInit(): void {}
}
