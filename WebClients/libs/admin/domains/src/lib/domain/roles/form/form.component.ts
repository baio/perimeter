import { RolesDataAccessService, Permission } from '@admin/data-access';
import { AdminForm } from '@admin/shared';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { getDefinition } from './form.definition';
import { Observable } from 'rxjs';

@Component({
    selector: 'admin-role-form',
    templateUrl: './form.component.html',
    styleUrls: ['./form.component.scss'],
})
export class RoleFormComponent {
    private readonly domainId: number;
    readonly definition: AdminForm.FormDefinition; // = definition;
    private readonly permissions$: Observable<Permission[]>;
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
        private readonly activatedRoute: ActivatedRoute,
        private readonly dataAccess: RolesDataAccessService,
        private readonly router: Router
    ) {
        this.domainId = +activatedRoute.parent.parent.snapshot.params['id'];
        this.permissions$ = dataAccess.loadPermissions(this.domainId);
        this.definition = getDefinition(this.permissions$);
    }
}
