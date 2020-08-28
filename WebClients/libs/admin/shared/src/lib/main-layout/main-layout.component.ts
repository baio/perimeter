import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Store } from '@ngrx/store';
import {
    Domain,
    selectProfileDomainsList,
    selectActiveDomain,
} from '@admin/profile';
import { map } from 'rxjs/operators';
import { Observable, merge, combineLatest } from 'rxjs';
import { Router } from '@angular/router';

export interface IView {
    activeDomain: Domain;
    domains: Domain[];
}

@Component({
    selector: 'admin-main-layout',
    templateUrl: './main-layout.component.html',
    styleUrls: ['./main-layout.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MainLayoutComponent implements OnInit {
    readonly view$: Observable<IView>;

    constructor(store: Store, private router: Router) {
        const activeDomain$ = store.select(selectActiveDomain(router.url));
        const domains$ = store.select(selectProfileDomainsList);
        this.view$ = combineLatest([activeDomain$, domains$]).pipe(
            map(([activeDomain, domains]) => {
                return {
                    activeDomain: activeDomain,
                    domains: domains.filter((f) => f.id !== activeDomain.id),
                };
            })
        );
    }

    ngOnInit(): void {}

    onSelectDomain(domain: Domain) {
        if (domain.isTenantManagement) {
            this.router.navigate(['/tenants', domain.tenant.id, 'domains']);
        } else {
            this.router.navigate(['/domains', domain.id, 'apps']);
        }
    }
}
