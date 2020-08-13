import {
    Component,
    OnInit,
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Inject,
} from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { FormValidators, HTTP_BASE_URL_CONFIG } from '@perimeter/common';
import { LoginParams, AuthDataAccessService } from '@ip/data-access';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
    selector: 'ip-login-page',
    templateUrl: './login-page.component.html',
    styleUrls: ['./login-page.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPageComponent implements OnInit {
    loginParams: LoginParams;
    errorMessage: string;
    queryEvent: string;
    public readonly form: FormGroup;

    get submitAction() {
        return `${this.baseUrl}/auth/login`;
    }

    constructor(
        fb: FormBuilder,
        @Inject(HTTP_BASE_URL_CONFIG) private readonly baseUrl: string,
        private readonly dataAccess: AuthDataAccessService,
        private readonly activatedRoute: ActivatedRoute,
        private readonly cdr: ChangeDetectorRef
    ) {
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

        if (errors.length > 0) {
            this.errorMessage = `Following parameters not defined: ${errors.join(
                ', '
            )}`;

            this.loginParams = {} as any;
        } else {
            this.loginParams = parsedParams;
        }

        this.form = fb.group({
            email: [null, [FormValidators.empty, Validators.email]],
            password: [null, [FormValidators.empty]],
            client_id: [this.loginParams.client_id],
            state: [this.loginParams.state],
            response_type: [this.loginParams.response_type],
            redirect_uri: [this.loginParams.redirect_uri],
            scope: [this.loginParams.scope],
            code_challenge: [this.loginParams.code_challenge],
            code_challenge_method: [this.loginParams.code_challenge_method],
        });

    }

    ngOnInit(): void {}

    onSubmit(form: HTMLFormElement) {
        form.submit();
    }
}