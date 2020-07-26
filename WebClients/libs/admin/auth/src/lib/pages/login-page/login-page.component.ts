import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';

@Component({
    selector: 'admin-login-page',
    templateUrl: './login-page.component.html',
    styleUrls: ['./login-page.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPageComponent implements OnInit {
    public readonly form: FormGroup;

    constructor(private fb: FormBuilder) {
        this.form = this.fb.group({
            email: [null, [Validators.required]],
            password: [null, [Validators.required]],
            remember: [true],
        });
    }

    submitForm(): void {}

    ngOnInit(): void {}
}
