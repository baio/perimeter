import { TenantsDataAccessService } from '@admin/data-access';
import { AdminForm } from '@admin/shared';
import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '@perimeter/ngx-auth';
import { switchMap } from 'rxjs/operators';
import { definition } from './create-tenant-form.definition';

@Component({
    selector: 'admin-create-tenant-form',
    templateUrl: './create-tenant-form.component.html',
    styleUrls: ['./create-tenant-form.component.scss'],
})
export class CreateTenantFormComponent {
    readonly definition = definition;

    readonly storeValueDataAccess: AdminForm.Data.StoreValueDataAccess = (
        item: any
    ) =>
        this.dataAccess.createItem(item).pipe(
            switchMap((tenantId) => {
                // authorize under created tenant
                return this.authService.authorize({
                    useSSO: true,
                    clientId: '__DEFAULT_CLIENT_ID__',
                    redirectPath: `/tenants/${tenantId}/domains`,
                });
            })
        );

    constructor(
        private readonly dataAccess: TenantsDataAccessService,
        private readonly authService: AuthService,
        private readonly activatedRoute: ActivatedRoute,
        private readonly router: Router
    ) {}

    onClose() {
        this.router.navigate(['..'], { relativeTo: this.activatedRoute });
    }
}
