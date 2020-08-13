import { DomainsDataAccessService } from '@admin/data-access';
import { AdminForm, AdminListService } from '@admin/shared';
import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { definition } from './create-env-form.definition';

@Component({
    selector: 'admin-create-env-domain-pool-form',
    templateUrl: './create-env-form.component.html',
    styleUrls: ['./create-env-form.component.scss'],
})
export class CreateEnvFormComponent {
    private readonly domainId: number;
    readonly definition = definition;

    readonly storeValueDataAccess: AdminForm.Data.StoreValueDataAccess = (
        item: any
    ) =>
        this.dataAccess
            .createEnvItem(this.domainId, item)
            .pipe(tap(() => this.listService.onRowUpdated(item)));

    constructor(
        private readonly activatedRoute: ActivatedRoute,
        private readonly router: Router,
        private readonly dataAccess: DomainsDataAccessService,
        private readonly listService: AdminListService
    ) {
        this.domainId = +activatedRoute.snapshot.params['id'];
    }

    onClose() {
        this.router.navigate(['.'], {
            relativeTo: this.activatedRoute.parent,
        });
    }
}
