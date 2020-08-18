import { AdminsDataAccessService } from '@admin/data-access';
import { AdminForm } from '@admin/shared';
import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { getDefinition } from './form.definition';

@Component({
    selector: 'admin-admin-form',
    templateUrl: './form.component.html',
    styleUrls: ['./form.component.scss'],
})
export class AdminFormComponent {
    private readonly domainId: number;
    readonly definition: AdminForm.FormDefinition;

    readonly loadValueDataAccess: AdminForm.Data.LoadValueDataAccess = (
        id: string
    ) => this.dataAccess.loadItem(this.domainId, id);

    readonly storeValueDataAccess: AdminForm.Data.StoreValueDataAccess = (
        item: any
    ) => this.dataAccess.updateItem(this.domainId, item);

    constructor(
        activatedRoute: ActivatedRoute,
        private readonly dataAccess: AdminsDataAccessService
    ) {
        this.domainId = +activatedRoute.parent.parent.snapshot.params['id'];
        this.definition = getDefinition(this.dataAccess.getAllRoles());
    }
}
