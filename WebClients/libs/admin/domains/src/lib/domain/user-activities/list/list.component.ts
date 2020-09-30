import { UserActivitiesDataAccessService } from '@admin/data-access';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { listDefinition } from './list.definition';

@Component({
    selector: 'admin-user-activities-list',
    templateUrl: './list.component.html',
    styleUrls: ['./list.component.scss'],
})
export class UserActivitiesListComponent implements OnInit {
    private readonly domainId: number;
    readonly listDefinition = listDefinition;
    readonly dataProvider: HlcNzTable.Data.DataProvider = (state) =>
        this.dataAccess.loadUsersList(this.domainId, state);

    constructor(
        activatedRoute: ActivatedRoute,
        private readonly dataAccess: UserActivitiesDataAccessService
    ) {
        this.domainId = +activatedRoute.parent.snapshot.params['id'];
    }

    ngOnInit(): void {}
}
