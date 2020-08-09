import {
    FormValidators,
    mapBadRequestResponseToFormValidationErrors,
} from '@perimeter/common';
import { AuthDataAccessService } from '@ip/data-access';
import { HttpErrorResponse } from '@angular/common/http';
import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    OnInit,
} from '@angular/core';
import {
    FormBuilder,
    FormControl,
    FormGroup,
    Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
    selector: 'ip-signup-page',
    templateUrl: './signup-page.component.html',
    styleUrls: ['./signup-page.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignupPageComponent implements OnInit {
    qs: string;
    isSubmitting = false;
    errorMessage: string;
    isAgreementVisible = false;

    public readonly form: FormGroup;

    constructor(
        private fb: FormBuilder,
        private readonly authDataAccess: AuthDataAccessService,
        private readonly cdr: ChangeDetectorRef,
        private readonly router: Router,
        private readonly activatedRoute: ActivatedRoute
    ) {
        this.form = this.fb.group({
            email: [null, [Validators.email, FormValidators.empty]],
            password: [
                null,
                [
                    FormValidators.empty,
                    Validators.minLength(6),
                    Validators.maxLength(100),
                    FormValidators.missUpperCaseLetter,
                    FormValidators.missLowerCaseLetter,
                    FormValidators.missDigit,
                    FormValidators.missSpecialChar,
                ],
            ],
            checkPassword: [
                null,
                [Validators.required, this.confirmationValidator],
            ],
            firstName: [null, [FormValidators.empty]],
            lastName: [null, [FormValidators.empty]],
            agree: [false],
        });
    }

    ngOnInit(): void {
        // login info in qs
        this.qs = window.location.search;
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

    async submitForm() {
        try {
            this.isSubmitting = true;
            await this.authDataAccess.signUp(this.form.value, this.qs).toPromise();
            this.errorMessage = null;
        } catch (_err) {
            const err = _err as HttpErrorResponse;
            if (err.status === 400) {
                this.errorMessage = 'Some fields have invalid data';
                mapBadRequestResponseToFormValidationErrors(
                    this.form,
                    err.error
                );
            } else {
                this.errorMessage = err.message;
            }
        } finally {
            this.isSubmitting = false;
            this.cdr.markForCheck();
        }
        if (!this.errorMessage) {
            this.router.navigate(['..', 'register-sent'], {
                relativeTo: this.activatedRoute,
            });
        }
    }
}
