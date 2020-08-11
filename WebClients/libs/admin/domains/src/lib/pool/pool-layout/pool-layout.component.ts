import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'admin-pool-layout',
    templateUrl: './pool-layout.component.html',
    styleUrls: ['./pool-layout.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PoolLayoutComponent implements OnInit {
    constructor() {}

    ngOnInit(): void {}

}
