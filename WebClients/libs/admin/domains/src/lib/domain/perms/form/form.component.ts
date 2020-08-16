import { PermissionsDataAccessService } from '@admin/data-access';
import { AdminForm } from '@admin/shared';
import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { definition } from './form.definition';

@Component({
    selector: 'admin-perm-form',
    templateUrl: './form.component.html',
    styleUrls: ['./form.component.scss'],
})
export class PermFormComponent {
    private readonly domainId: number;
    readonly definition = definition;
    readonly loadValueDataAccess: AdminForm.Data.LoadValueDataAccess = (
        id: number
    ) => this.dataAccess.loadItem(this.domainId, id);

    readonly storeValueDataAccess: AdminForm.Data.StoreValueDataAccess = (
        item: any
    ) =>
        item.id
            ? this.dataAccess.updateItem(this.domainId, item.id, item)
            : this.dataAccess.createItem(this.domainId, item);

    constructor(
        activatedRoute: ActivatedRoute,
        private readonly dataAccess: PermissionsDataAccessService
    ) {
        this.domainId = +activatedRoute.parent.parent.snapshot.params['id'];
    }
}
