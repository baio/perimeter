import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { AuthService } from '@perimeter/ngx-auth';

@Component({
    selector: 'admin-home-page',
    templateUrl: './home-page.component.html',
    styleUrls: ['./home-page.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomePageComponent implements OnInit {
    constructor(private readonly authService: AuthService) {}

    ngOnInit(): void {}

    async onLogin() {
        const loginUrl = await this.authService.createLoginUrl();
        if (!window['Cypress']) {
            window.location.href = loginUrl;
        } else {
            console.warn('cypress env dont redirect');
        }
    }
}
