import {
    ApisDataAccessService,
    PermissionsDataAccessService,
} from '@admin/data-access';
import { AdminList } from '@admin/shared';
import { Component, OnInit } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { listDefinition } from './list.definition';

@Component({
    selector: 'admin-perms-list',
    templateUrl: './list.component.html',
    styleUrls: ['./list.component.scss'],
})
export class PermsListComponent implements OnInit {
    readonly listDefinition = listDefinition;
    readonly dataProvider: HlcNzTable.Data.DataProvider = (state) =>
        this.dataAccess.loadList(state);
    readonly removeItemDataAccess: AdminList.Data.RemoveItemDataAccess = ({
        id,
    }) => this.dataAccess.removeItem(id);

    constructor(private readonly dataAccess: PermissionsDataAccessService) {}

    ngOnInit(): void {}
}
