import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Store } from '@ngrx/store';
import {
    Domain,
    selectProfileDomainsList,
    selectActiveDomain,
    ProfileState,
    selectUser,
    User,
} from '@admin/profile';
import { map } from 'rxjs/operators';
import { Observable, merge, combineLatest } from 'rxjs';
import { Router } from '@angular/router';
import { AuthService } from '@perimeter/ngx-auth';
import { TranslocoService } from '@ngneat/transloco';

export interface IView {
    activeDomain: Domain;
    domains: Domain[];
    user: User;
}

@Component({
    selector: 'admin-main-layout',
    templateUrl: './main-layout.component.html',
    styleUrls: ['./main-layout.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MainLayoutComponent implements OnInit {
    readonly view$: Observable<IView>;

    constructor(
        store: Store,
        private router: Router,
        private readonly authService: AuthService,
        private readonly transloco: TranslocoService
    ) {
        const activeDomain$ = store.select(selectActiveDomain(router.url));
        const domains$ = store.select(selectProfileDomainsList);
        const user$ = store.select(selectUser);
        this.view$ = combineLatest([activeDomain$, domains$, user$]).pipe(
            map(([activeDomain, domains, user]) => {
                return {
                    user,
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

    onLogout() {
        this.authService.logout();
    }

    onSetLanguage(lang: string) {
        localStorage.setItem('APP_LANG', lang);
        this.transloco.setActiveLang(lang);
    }
}
