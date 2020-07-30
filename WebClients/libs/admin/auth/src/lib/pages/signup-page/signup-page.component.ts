import { FormValidators } from '@admin/common';
import { AuthDataAccessService } from '@admin/data-access';
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
    selector: 'admin-signup-page',
    templateUrl: './signup-page.component.html',
    styleUrls: ['./signup-page.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignupPageComponent implements OnInit {
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
                    FormValidators.regex(/[A-Z]/, {
                        ['miss-upper-case-letter']: true,
                    }),
                    FormValidators.regex(/[a-z]/, {
                        ['miss-lower-case-letter']: true,
                    }),
                    FormValidators.regex(/[0-9]/, { ['miss-digit']: true }),
                    FormValidators.regex(/[!@#$%^&*()_+=\[{\]};:<>|./?,-]/, {
                        ['miss-special-char']: true,
                    }),
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

    ngOnInit(): void {}

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
            await this.authDataAccess.signUp(this.form.value).toPromise();
            this.errorMessage = null;
        } catch (err) {
            this.errorMessage = (err as HttpErrorResponse).message;
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
