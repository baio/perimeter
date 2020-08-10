import { Component, OnInit } from '@angular/core';
import { listDefinition } from './list.definition';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { AdminList } from '@admin/shared';
import { DomainsDataAccessService } from '@admin/data-access';

@Component({
    selector: 'admin-domains-list',
    templateUrl: './list.component.html',
    styleUrls: ['./list.component.scss'],
})
export class ListComponent implements OnInit {
    readonly listDefinition = listDefinition;
    readonly dataProvider: HlcNzTable.Data.DataProvider = (state) =>
        this.dataAccess.loadList(state);
    readonly removeItemDataAccess: AdminList.Data.RemoveItemDataAccess = ({
        id,
    }) => this.dataAccess.removeItem(id);

    constructor(private readonly dataAccess: DomainsDataAccessService) {}

    ngOnInit(): void {}
}
