import {
    Component,
    OnInit,
    ChangeDetectionStrategy,
    ChangeDetectorRef,
} from '@angular/core';
import { AuthService, LoginResult } from '@perimeter/ngx-auth';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
    selector: 'admin-login-cb-page',
    templateUrl: './login-cb-page.component.html',
    styleUrls: ['./login-cb-page.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginCbPageComponent implements OnInit {
    errorMessage: string;

    constructor(
        private readonly authService: AuthService,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly cdr: ChangeDetectorRef
    ) {}

    async ngOnInit() {
        const loginResult = this.authService.parseLoginRedirect(
            this.route.snapshot.queryParams
        );
        if (loginResult.kind === 'ok') {
            try {
                const redirectPath = await this.authService.token(
                    loginResult.code,
                    loginResult.state
                );
                this.router.navigateByUrl(redirectPath || '/');
            } catch (_err) {
                const err = _err as HttpErrorResponse;
                this.errorMessage = err.message || 'Unknown Error';
                this.cdr.markForCheck();
            }
        } else {
            if (loginResult.error === 'login_required') {
                window.location.href = await this.authService.createLoginUrl();
            } else {
                this.errorMessage = loginResult.error;
            }
        }
    }
}
