import { AdminsDataAccessService, TenantAdminsDataAccessService } from '@admin/data-access';
import { AdminForm } from '@admin/shared';
import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { getDefinition } from './form.definition';

@Component({
    selector: 'admin-tenant-admin-form',
    templateUrl: './form.component.html',
    styleUrls: ['./form.component.scss'],
})
export class TenantAdminFormComponent {

    readonly definition: AdminForm.FormDefinition;

    readonly loadValueDataAccess: AdminForm.Data.LoadValueDataAccess = (
        id: string
    ) => this.dataAccess.loadItem(id);

    readonly storeValueDataAccess: AdminForm.Data.StoreValueDataAccess = (
        item: any
    ) => this.dataAccess.updateItem(item);

    constructor(
        activatedRoute: ActivatedRoute,
        private readonly dataAccess: TenantAdminsDataAccessService
    ) {
        this.definition = getDefinition(
            this.dataAccess.getAllRoles()
        );
    }
}
