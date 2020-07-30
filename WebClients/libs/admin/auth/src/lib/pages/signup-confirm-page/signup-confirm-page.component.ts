import {
    Component,
    OnInit,
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    AfterViewInit,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthDataAccessService } from '@admin/data-access';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
    selector: 'admin-signup-confirm-page',
    templateUrl: './signup-confirm-page.component.html',
    styleUrls: ['./signup-confirm-page.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignupConfirmPageComponent implements OnInit {
    errorMessage: string;

    constructor(
        private readonly activatedRoute: ActivatedRoute,
        private readonly router: Router,
        private readonly authDataAccess: AuthDataAccessService,
        private readonly cdr: ChangeDetectorRef
    ) {}

    ngOnInit() {
        const token = this.activatedRoute.snapshot.queryParams['token'];
        if (!token) {
            this.errorMessage = 'Token is not found in query string';
            return;
        }

        this.authDataAccess
            .signUpConfirm(token)
            .toPromise()
            .then(() =>
                this.router.navigate(
                    ['..', 'login', { event: 'sign-up-confirm-success' }],
                    { relativeTo: this.activatedRoute }
                )
            )
            .catch((_err) => {
                const err = _err as HttpErrorResponse;
                if (err.message) {
                    this.errorMessage = err.message;
                    this.cdr.detectChanges();
                }
            });
    }
}
