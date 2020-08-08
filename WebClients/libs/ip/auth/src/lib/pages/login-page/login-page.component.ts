import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';

@Component({
    selector: 'ip-login-page',
    templateUrl: './login-page.component.html',
    styleUrls: ['./login-page.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPageComponent implements OnInit {
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
        this.queryEvent = this.activatedRoute.snapshot.params['event'];
    }
}
