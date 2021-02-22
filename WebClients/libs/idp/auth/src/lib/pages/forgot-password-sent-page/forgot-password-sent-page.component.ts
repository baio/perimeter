import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { PreservedQueryParamsService } from '../services';

@Component({
    selector: 'ip-forgot-password-sent-page',
    templateUrl: './forgot-password-sent-page.component.html',
    styleUrls: ['./forgot-password-sent-page.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ForgotPasswordSentPageComponent implements OnInit {
    constructor(readonly preservedQueryParams: PreservedQueryParamsService) {}

    ngOnInit(): void {}
}
