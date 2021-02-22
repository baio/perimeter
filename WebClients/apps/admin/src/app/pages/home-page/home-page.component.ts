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
    message: string;

    constructor(
        private readonly authService: AuthService,
        activatedRoute: ActivatedRoute
    ) {
        const queryEvent = activatedRoute.snapshot.queryParamMap.get('event');

        // TODO : Any request with event query will be dispatched to idp
        switch (queryEvent) {
            case 'reset-password-success':
                this.message = 'Password reset! You can login now.';
            case 'sign-up-confirm-success':
                this.message = 'Email confirmed! You can login now.';
        }

        if (queryEvent) {
            this.onLogin(queryEvent);
        }
    }

    ngOnInit(): void {}

    async onLogin(queryEvent?: string) {
        const loginUrl = await this.authService.createLoginUrl({
            useSSO: true,
            redirectPath: REDIRECT_PATH,
        });
        // Simple delegate handling of `event` query string to the idp
        window.location.href = loginUrl + (queryEvent ? '&event=' + queryEvent : '');
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
