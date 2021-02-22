import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { PreservedQueryParamsService } from '../services';

@Component({
    selector: 'ip-signup-confirm-sent-page',
    templateUrl: './signup-confirm-sent-page.component.html',
    styleUrls: ['./signup-confirm-sent-page.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignupConfirmSentPageComponent implements OnInit {
    constructor(readonly preservedQueryParams: PreservedQueryParamsService) {}

    ngOnInit(): void {}
}
