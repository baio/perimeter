import {
    SocialConnection,
    SocialConnectionsDataAccessService,
} from '@admin/data-access';
import { AdminForm } from '@admin/shared';
import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { definition } from './form.definition';

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
        item: SocialConnection,
        current: SocialConnection
    ) => {
        if (item.isEnabled && !current.isEnabled) {
            return this.dataAccess.createItem(this.domainId, item);
        } else if (!item.isEnabled && current.isEnabled) {
            return this.dataAccess.removeItem(this.domainId, item.id);
        } else if (item.isEnabled && current.isEnabled) {
            return this.dataAccess.updateItem(this.domainId, item);
        } else {
            return of(null);
        }
    };

    constructor(
        private readonly activatedRoute: ActivatedRoute,
        private readonly dataAccess: SocialConnectionsDataAccessService,
        private readonly router: Router
    ) {
        this.domainId = +activatedRoute.parent.parent.snapshot.params['id'];
    }
}
