import {
    Component,
    OnInit,
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    AfterViewInit,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthDataAccessService } from '@idp/data-access';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
    selector: 'ip-signup-confirm-page',
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

    async ngOnInit() {
        const token = this.activatedRoute.snapshot.queryParams['token'];
        const redirectUri = this.activatedRoute.snapshot.queryParams[
            'redirect_uri'
        ];
        if (!token) {
            this.errorMessage = 'Token is not found in query string';
            return;
        }

        await this.authDataAccess
            .signUpConfirm(token)
            .toPromise()
            .then(
                () =>
                
                    (window.location.href =
                        redirectUri + '?event=sign-up-confirm-success')
                
                /*
                this.router.navigate(
                    ['..', 'login', { event: 'sign-up-confirm-success' }],
                    {
                        relativeTo: this.activatedRoute,
                        preserveQueryParams: true,
                    }
                )
                */
                
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
