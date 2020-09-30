import { UserActivitiesDataAccessService } from '@admin/data-access';
import { AdminList } from '@admin/shared';
import { Component, OnInit } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { listDefinition } from './list.definition';
import { ActivatedRoute } from '@angular/router';

@Component({
    selector: 'admin-admin-activities-list',
    templateUrl: './list.component.html',
    styleUrls: ['./list.component.scss'],
})
export class AdminActivitiesListComponent implements OnInit {
    private readonly domainId: number;
    readonly listDefinition = listDefinition;
    readonly dataProvider: HlcNzTable.Data.DataProvider = (state) =>
        this.dataAccess.loadAdminsList(this.domainId, state);

    constructor(
        activatedRoute: ActivatedRoute,
        private readonly dataAccess: UserActivitiesDataAccessService
    ) {
        this.domainId = +activatedRoute.parent.snapshot.params['id'];
    }

    ngOnInit(): void {}
}
