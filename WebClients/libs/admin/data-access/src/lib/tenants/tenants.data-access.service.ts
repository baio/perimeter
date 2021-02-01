import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable()
export class TenantsDataAccessService {
    constructor(private readonly http: HttpClient) {}

    createItem(data: { name: string }): Observable<any> {
        return this.http.post('tenant/tenants', data);
    }
}
