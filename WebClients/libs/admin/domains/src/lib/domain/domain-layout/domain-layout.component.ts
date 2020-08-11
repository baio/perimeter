import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'admin-domain-layout',
    templateUrl: './domain-layout.component.html',
    styleUrls: ['./domain-layout.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DomainLayoutComponent implements OnInit {
    constructor() {}

    ngOnInit(): void {}

}
