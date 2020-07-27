import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import {
    FormGroup,
    FormBuilder,
    FormControl,
    Validators,
} from '@angular/forms';

const emptyValidator = (control: FormControl) => {
    const isEmpty = !control.value || /^\s+$/.test(control.value as string);
    return isEmpty && { empty: true };
};
@Component({
    selector: 'admin-signup-page',
    templateUrl: './signup-page.component.html',
    styleUrls: ['./signup-page.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignupPageComponent implements OnInit {
    isAgreementVisible = false;

    public readonly form: FormGroup;

    constructor(private fb: FormBuilder) {
        this.form = this.fb.group({
            email: [null, [Validators.email, emptyValidator]],
            password: [
                null,
                [
                    emptyValidator,
                    Validators.minLength(6),
                    Validators.maxLength(100),
                ],
            ],
            checkPassword: [
                null,
                [Validators.required, this.confirmationValidator],
            ],
            firstName: [null, [emptyValidator]],
            lastName: [null, [emptyValidator]],
            agree: [false],
        });
    }

    submitForm(): void {}

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

    getCaptcha(e: MouseEvent): void {
        e.preventDefault();
    }
}
