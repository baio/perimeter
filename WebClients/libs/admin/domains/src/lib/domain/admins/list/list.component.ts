import {
    ApisDataAccessService,
    AdminsDataAccessService,
    UserRole,
    USER_ROLE_DOMAIN_OWNER_ID,
} from '@admin/data-access';
import { AdminList } from '@admin/shared';
import { Component, OnInit } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { listDefinition } from './list.definition';
import { ActivatedRoute } from '@angular/router';

@Component({
    selector: 'admin-admins-list',
    templateUrl: './list.component.html',
    styleUrls: ['./list.component.scss'],
})
export class AdminsListComponent implements OnInit {
    private readonly domainId: number;
    readonly listDefinition = listDefinition;
    readonly dataProvider: HlcNzTable.Data.DataProvider = (state) =>
        this.dataAccess.loadList(this.domainId, state);
    readonly removeItemDataAccess: AdminList.Data.RemoveItemDataAccess = ({
        id,
    }) => {
        return this.dataAccess.removeItem(this.domainId, id);
    };

    readonly canRemoveRow: AdminList.CheckRowFun<UserRole> = (row) =>
        row.roles.some((x) => x.id !== USER_ROLE_DOMAIN_OWNER_ID);

    readonly beforeRowClick: AdminList.CheckRowFun<UserRole> = (row) =>
        row.roles.some((x) => x.id !== USER_ROLE_DOMAIN_OWNER_ID);

    constructor(
        activatedRoute: ActivatedRoute,
        private readonly dataAccess: AdminsDataAccessService
    ) {
        this.domainId = +activatedRoute.parent.snapshot.params['id'];
    }

    ngOnInit(): void {}
}
