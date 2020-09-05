import { Component, OnInit } from '@angular/core';
import { getDefinition } from './form.definition';
import { AdminForm, isNew$ } from '@admin/shared';
import { ActivatedRoute, Router } from '@angular/router';
import { DomainsDataAccessService } from '@admin/data-access';
import { of } from 'rxjs';
import { Store } from '@ngrx/store';
import { tap } from 'rxjs/operators';
import { loadManagementDomains } from '@admin/profile';

@Component({
    selector: 'admin-domain-pool-form',
    templateUrl: './form.component.html',
    styleUrls: ['./form.component.scss'],
})
export class DomainPoolFormComponent {
    readonly definition = getDefinition(isNew$(this.activatedRoute));

    readonly storeValueDataAccess: AdminForm.Data.StoreValueDataAccess = (
        item: any
    ) =>
        (item.id
            ? this.dataAccess.updateItem(item.id, item)
            : this.dataAccess.createItem(item)
        ).pipe(tap(() => this.store.dispatch(loadManagementDomains())));

    readonly loadValueDataAccess: AdminForm.Data.LoadValueDataAccess = (
        id: number
    ) => (id ? this.dataAccess.loadItem(id) : of({}));

    constructor(
        private readonly store: Store,
        private readonly activatedRoute: ActivatedRoute,
        private readonly dataAccess: DomainsDataAccessService,
        private readonly router: Router
    ) {}
}
