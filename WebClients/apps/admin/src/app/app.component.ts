import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
    selector: 'admin-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
    constructor(private readonly http: HttpClient) {}

    async ngOnInit() {
        const authResult = await this.http.get('auth/version').toPromise();
        console.log('auth API', authResult);
        const tenantResult = await this.http.get('tenant/version').toPromise();
        console.log('tenant API', tenantResult);
    }
}
