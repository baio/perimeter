import { DomainsDataAccessService } from '@admin/data-access';
import { AdminForm, AdminListService } from '@admin/shared';
import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { definition } from './create-env-form.definition';
import { Store } from '@ngrx/store';
import { loadManagementDomains } from '@admin/profile';

@Component({
    selector: 'admin-create-env-domain-pool-form',
    templateUrl: './create-env-form.component.html',
    styleUrls: ['./create-env-form.component.scss'],
})
export class CreateEnvFormComponent {
    private readonly domainId = +this.activatedRoute.snapshot.params['id'];
    readonly definition = definition;

    readonly storeValueDataAccess: AdminForm.Data.StoreValueDataAccess = (
        item: any
    ) =>
        this.dataAccess.createEnvItem(this.domainId, item).pipe(
            tap(() => {
                this.store.dispatch(loadManagementDomains());
                this.listService.onRowUpdated(item);
            })
        );

    constructor(
        private readonly store: Store,
        private readonly activatedRoute: ActivatedRoute,
        private readonly router: Router,
        private readonly dataAccess: DomainsDataAccessService,
        private readonly listService: AdminListService
    ) {}

    onClose() {
        this.router.navigate(['.'], {
            relativeTo: this.activatedRoute.parent,
        });
    }
}
