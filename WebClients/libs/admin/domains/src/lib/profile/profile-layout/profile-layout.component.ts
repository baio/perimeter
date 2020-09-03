import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'admin-profile-layout',
    templateUrl: './profile-layout.component.html',
    styleUrls: ['./profile-layout.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfileLayoutComponent implements OnInit {
    constructor() {}

    ngOnInit(): void {}

}
