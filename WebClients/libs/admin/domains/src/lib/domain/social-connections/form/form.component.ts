import {
    RolesDataAccessService,
    Permission,
    SocialConnectionsDataAccessService,
} from '@admin/data-access';
import { AdminForm } from '@admin/shared';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { definition } from './form.definition';
import { Observable } from 'rxjs';

@Component({
    selector: 'admin-social-connections-form',
    templateUrl: './form.component.html',
    styleUrls: ['./form.component.scss'],
})
export class SocialConnectionsFormComponent {
    private readonly domainId: number;
    readonly definition = definition;
    readonly loadValueDataAccess: AdminForm.Data.LoadValueDataAccess = (
        id: string
    ) => this.dataAccess.loadItem(this.domainId, id);

    readonly storeValueDataAccess: AdminForm.Data.StoreValueDataAccess = (
        item: any
    ) =>
        item.id
            ? this.dataAccess.updateItem(this.domainId, item)
            : this.dataAccess.createItem(this.domainId, item);

    constructor(
        private readonly activatedRoute: ActivatedRoute,
        private readonly dataAccess: SocialConnectionsDataAccessService,
        private readonly router: Router
    ) {
        this.domainId = +activatedRoute.parent.parent.snapshot.params['id'];
    }
}
