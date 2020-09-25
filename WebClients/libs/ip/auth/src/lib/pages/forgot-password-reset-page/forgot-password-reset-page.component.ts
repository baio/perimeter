import {
    Component,
    OnInit,
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    AfterViewInit,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthDataAccessService } from '@ip/data-access';
import { HttpErrorResponse } from '@angular/common/http';
import {
    FormGroup,
    FormBuilder,
    Validators,
    FormControl,
} from '@angular/forms';
import { FormValidators } from '@perimeter/common';

@Component({
    selector: 'ip-forgot-password-reset-page',
    templateUrl: './forgot-password-reset-page.component.html',
    styleUrls: ['./forgot-password-reset-page.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ForgotPasswordResetPageComponent implements OnInit {
    isSubmitting = false;
    errorMessage: string;
    public readonly form: FormGroup;

    private get token() {
        return this.activatedRoute.snapshot.queryParams['token'];
    }

    constructor(
        fb: FormBuilder,
        private readonly activatedRoute: ActivatedRoute,
        private readonly router: Router,
        private readonly authDataAccess: AuthDataAccessService,
        private readonly cdr: ChangeDetectorRef
    ) {
        this.form = fb.group({
            password: [
                null,
                [
                    FormValidators.empty,
                    Validators.minLength(6),
                    Validators.maxLength(100),
                    FormValidators.password,
                ],
            ],
            checkPassword: [
                null,
                [Validators.required, this.confirmationValidator],
            ],
        });
    }

    updateConfirmValidator(): void {
        /** wait for refresh value */
        Promise.resolve().then(() =>
            this.form.controls.checkPassword.updateValueAndValidity()
        );
    }

    confirmationValidator = (
        control: FormControl
    ): { [s: string]: boolean } => {
        if (!control.value) {
            return { required: true };
        } else if (control.value !== this.form.controls.password.value) {
            return { confirm: true, error: true };
        }
        return {};
    };

    ngOnInit() {
        if (!this.token) {
            this.errorMessage = 'Token is not found in query string';
            return;
        }
    }

    async submitForm() {
        try {
            this.isSubmitting = true;
            await this.authDataAccess
                .resetPasswordConfirm({
                    password: this.form.value.password,
                    token: this.token,
                })
                .toPromise();
            this.errorMessage = null;
        } catch (_err) {
            const err = _err as HttpErrorResponse;
            this.errorMessage = err.message;
        } finally {
            this.isSubmitting = false;
            this.cdr.markForCheck();
        }
        if (!this.errorMessage) {
            this.router.navigate(['/home', { event: 'reset-password-success' }], {
                relativeTo: this.activatedRoute,
                preserveQueryParams: true,
            });
        }
    }
}
