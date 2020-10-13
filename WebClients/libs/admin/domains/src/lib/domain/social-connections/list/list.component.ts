import {
    ApisDataAccessService,
    RolesDataAccessService,
    SocialConnectionsDataAccessService,
} from '@admin/data-access';
import { AdminList } from '@admin/shared';
import { Component, OnInit } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { listDefinition } from './list.definition';
import { ActivatedRoute } from '@angular/router';
import { map } from 'rxjs/operators';

@Component({
    selector: 'admin-social-connections-list',
    templateUrl: './list.component.html',
    styleUrls: ['./list.component.scss'],
})
export class SocialConnectionsListComponent implements OnInit {
    private readonly domainId: number;
    readonly listDefinition = listDefinition;
    readonly dataProvider: HlcNzTable.Data.DataProvider = (state) =>
        this.dataAccess.loadList(this.domainId, state).pipe(
            map((data) => ({
                data,
                pager: {
                    size: data.length,
                    total: data.length,
                    index: 1,
                },
            }))
        );

    constructor(
        activatedRoute: ActivatedRoute,
        private readonly dataAccess: SocialConnectionsDataAccessService
    ) {
        this.domainId = +activatedRoute.parent.snapshot.params['id'];
    }

    ngOnInit(): void {}
}
