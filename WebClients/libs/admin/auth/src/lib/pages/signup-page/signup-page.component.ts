import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import {
    FormGroup,
    FormBuilder,
    FormControl,
    Validators,
} from '@angular/forms';

@Component({
    selector: 'admin-signup-page',
    templateUrl: './signup-page.component.html',
    styleUrls: ['./signup-page.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignupPageComponent implements OnInit {
    public readonly form: FormGroup;

    constructor(private fb: FormBuilder) {
        this.form = this.fb.group({
            email: [null, [Validators.email, Validators.required]],
            password: [null, [Validators.required]],
            checkPassword: [
                null,
                [Validators.required, this.confirmationValidator],
            ],
            firstName: [null, [Validators.required]],
            lastName: [null, [Validators.required]],
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
