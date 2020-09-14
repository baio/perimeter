import {
    Component,
    OnInit,
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Inject,
    ViewChild,
    AfterViewInit,
} from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { FormValidators, HTTP_BASE_URL_CONFIG } from '@perimeter/common';
import { LoginParams, AuthDataAccessService, AppInfo } from '@ip/data-access';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';

@Component({
    selector: 'ip-login-page',
    templateUrl: './login-page.component.html',
    styleUrls: ['./login-page.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPageComponent implements OnInit, AfterViewInit {
    // Hide Form UI till data init
    loginParams: LoginParams;
    errorMessage: string;
    queryEvent: string;
    readonly form: FormGroup;
    appInfo: AppInfo;

    @ViewChild('submitForm') formElement: HTMLFormElement;

    get submitAction() {
        return `${this.baseUrl}/auth/login`;
    }

    constructor(
        fb: FormBuilder,
        @Inject(HTTP_BASE_URL_CONFIG) private readonly baseUrl: string,
        dataAccess: AuthDataAccessService,
        private readonly activatedRoute: ActivatedRoute,
        readonly cdr: ChangeDetectorRef
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

            const clientId = urlQueryParams['client_id'];

            if (!!clientId) {
                if (clientId === '__DEFAULT_CLIENT_ID__') {
                    this.appInfo = {
                        title: 'Perimeter',
                    };
                } else {
                    dataAccess
                        .getAppInfo(clientId)
                        .toPromise()
                        .then((res) => {
                            this.appInfo = res;
                            cdr.markForCheck();
                        });
                }
            }
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
            prompt: [urlQueryParams['prompt']],
        });
    }

    ngOnInit(): void {}

    ngAfterViewInit(): void {
        const urlQueryParams = this.activatedRoute.snapshot.queryParams;
        const formContainer = document.getElementById(
            'login_form_container'
        ) as HTMLFormElement;
        const form = document.getElementById('login_form') as HTMLFormElement;
        if (urlQueryParams['prompt'] === 'none') {
            form.submit();
        } else {
            formContainer.style.display = 'block';
        }
    }

    async onSubmit(form: HTMLFormElement) {
        // await this.dataAccess.assignSSO().toPromise();
        form.submit();
    }
}
