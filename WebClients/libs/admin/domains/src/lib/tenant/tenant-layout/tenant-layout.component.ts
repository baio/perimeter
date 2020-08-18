import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'admin-tenant-layout',
    templateUrl: './tenant-layout.component.html',
    styleUrls: ['./tenant-layout.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TenantLayoutComponent implements OnInit {
    constructor() {}

    ngOnInit(): void {}

}
