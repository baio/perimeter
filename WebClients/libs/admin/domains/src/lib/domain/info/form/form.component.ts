import {
    ApisDataAccessService,
    DomainsDataAccessService,
} from '@admin/data-access';
import { selectActiveDomain } from '@admin/profile';
import { AdminForm, isNew$ } from '@admin/shared';
import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { take } from 'rxjs/operators';
import { definition } from './form.definition';

@Component({
    selector: 'admin-api-form',
    templateUrl: './form.component.html',
    styleUrls: ['./form.component.scss'],
})
export class DomainFormComponent {
    private domainPoolId: number;
    private domainId: number;
    readonly definition = definition;
    // TODO : id ?
    readonly loadValueDataAccess: AdminForm.Data.LoadValueDataAccess = (
        id: number
    ) => this.dataAccess.loadEnvItem(this.domainPoolId, this.domainId);

    readonly storeValueDataAccess: AdminForm.Data.StoreValueDataAccess = (
        item: any
    ) => this.dataAccess.updateEnvItem(this.domainPoolId, this.domainId, item);

    constructor(
        private readonly activatedRoute: ActivatedRoute,
        private readonly dataAccess: DomainsDataAccessService,
        router: Router,
        store: Store
    ) {
        store
            .select(selectActiveDomain(router.url))
            .pipe(take(1))
            .subscribe((res) => {
                this.domainId = res.id;
                this.domainPoolId = res.pool.id;
            });
    }
}
