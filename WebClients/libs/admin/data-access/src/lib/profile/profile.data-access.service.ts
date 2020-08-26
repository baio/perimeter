import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Domain } from './models';

@Injectable()
export class ProfileDataAccessService {
    constructor(private readonly http: HttpClient) {}

    loadManagementDomains(): Observable<Domain[]> {
        return this.http.get<Domain[]>('/me/management/domains');
    }
}
