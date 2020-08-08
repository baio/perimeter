import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { getLoginParamsFromUrl } from '../../auth-service/utils';
import { LoginParams } from '../../auth-service/models';

@Component({
    selector: 'ip-login-page',
    templateUrl: './login-page.component.html',
    styleUrls: ['./login-page.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPageComponent implements OnInit {
    errorMessage: string;
    queryEvent: string;
    public readonly form: FormGroup;

    constructor(
        fb: FormBuilder,
        private readonly activatedRoute: ActivatedRoute
    ) {
        this.form = fb.group({
            email: [null, [Validators.required]],
            password: [null, [Validators.required]],
            remember: [true],
        });
    }

    submitForm(): void {}

    ngOnInit(): void {
        const urlParams = this.activatedRoute.snapshot.params;
        const urlQueryParams = this.activatedRoute.snapshot.queryParams;

        this.queryEvent = urlParams['event'];

        const parsedParams: LoginParams = {
            client_id: urlQueryParams['client_id'],
            response_type: urlQueryParams['response_type'] as any,
            state: urlQueryParams['state'],
            redirect_uri: urlQueryParams['redirect_uri'],
            scope: urlQueryParams['scope'],
            code_challenge: urlQueryParams['code_challenge'],
            code_challenge_method: urlQueryParams[
                'code_challenge_method'
            ] as any,
        };

        const errors = [];

        if (!parsedParams.client_id) {
            errors.push('client_id');
        }
        if (!parsedParams.response_type) {
            errors.push('response_type');
        }
        if (!parsedParams.redirect_uri) {
            errors.push('redirect_uri');
        }
        if (!parsedParams.scope) {
            errors.push('scope');
        }
        if (!parsedParams.code_challenge) {
            errors.push('code_challenge');
        }
        if (!parsedParams.code_challenge_method) {
            errors.push('code_challenge_method');
        }

        console.log(parsedParams);

        if (errors.length > 0) {
            this.errorMessage = `Following parameters not defined: ${errors.join(
                ', '
            )}`;
        }

        console.log('???', this.errorMessage);
    }
}
