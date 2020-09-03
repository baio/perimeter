import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
    selector: 'admin-profile-home',
    templateUrl: './profile-home.component.html',
    styleUrls: ['./profile-home.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfileHomeComponent implements OnInit {
    constructor(
        private readonly router: Router,
        private readonly activatedRoute: ActivatedRoute
    ) {}

    ngOnInit(): void {}

    onCreateTenant() {
        this.router.navigate(['.', 'create-tenant'], {
            relativeTo: this.activatedRoute,
        });
    }
}
