import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '@perimeter/ngx-auth';

const REDIRECT_PATH = '/';

@Component({
    selector: 'admin-home-page',
    templateUrl: './home-page.component.html',
    styleUrls: ['./home-page.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomePageComponent implements OnInit {
    queryEvent: string;

    constructor(
        private readonly authService: AuthService,
        activatedRoute: ActivatedRoute
    ) {
        this.queryEvent = activatedRoute.snapshot.params['event'];
        const autoLogin = activatedRoute.snapshot.queryParamMap['auto-login'];
        if (autoLogin) {
            this.onLogin();
        }
    }

    ngOnInit(): void {}

    async onLogin() {
        const loginUrl = await this.authService.createLoginUrl({
            useSSO: true,
            redirectPath: REDIRECT_PATH,
        });
        window.location.href = loginUrl;
    }

    async onSignUp() {
        const loginUrl = await this.authService.createSignUpUrl({
            redirectPath: REDIRECT_PATH,
        });
        window.location.href = loginUrl;
    }

    onLogout() {
        this.authService.logout();
    }
}
