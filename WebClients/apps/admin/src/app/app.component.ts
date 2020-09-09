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
        const result = await this.http.get('version').toPromise();
        console.log('API', result);
    }
}
