import {
    Component,
    OnInit,
    ChangeDetectionStrategy,
    ChangeDetectorRef,
} from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { FormValidators } from '@perimeter/common';
import { AuthDataAccessService } from '@ip/data-access';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
    selector: 'ip-forgot-password-page',
    templateUrl: './forgot-password-page.component.html',
    styleUrls: ['./forgot-password-page.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ForgotPasswordPageComponent implements OnInit {
    isSubmitting = false;
    errorMessage: string;
    public readonly form: FormGroup;

    constructor(
        fb: FormBuilder,
        private readonly authDataAccess: AuthDataAccessService,
        private readonly cdr: ChangeDetectorRef,
        private readonly router: Router,
        private readonly activatedRoute: ActivatedRoute
    ) {
        this.form = fb.group({
            email: [null, [FormValidators.empty, Validators.email]],
        });
    }

    async submitForm() {
        try {
            this.isSubmitting = true;
            await this.authDataAccess
                .resetPassword(this.form.value)
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
            this.router.navigate(['..', 'forgot-password-sent'], {
                relativeTo: this.activatedRoute,
                queryParamsHandling: 'preserve',
            });
        }
    }

    ngOnInit(): void {}
}
