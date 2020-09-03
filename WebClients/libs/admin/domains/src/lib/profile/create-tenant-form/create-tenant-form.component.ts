import { TenantsDataAccessService } from '@admin/data-access';
import { AdminForm } from '@admin/shared';
import { Component } from '@angular/core';
import { tap } from 'rxjs/operators';
import { definition } from './create-tenant-form.definition';
import { AuthService } from '@perimeter/ngx-auth';
import { ActivatedRoute, Router } from '@angular/router';

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
            tap(() => {
                // authorize under created tenant
                this.authService.authorize();
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
