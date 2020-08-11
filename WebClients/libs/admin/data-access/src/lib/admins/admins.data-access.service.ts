import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { Observable, of } from 'rxjs';
import { User } from '../models';
import { mapListRequestParams } from '../utils';

@Injectable()
export class AdminsDataAccessService {
    constructor(private readonly http: HttpClient) {}

    loadList(
        searchParams: HlcNzTable.Data.DataProviderState
    ): Observable<HlcNzTable.Data.DataProviderResult<User>> {
        const params = mapListRequestParams(searchParams);
        if (searchParams.filter && searchParams.filter.text) {
            params['filter.name'] = searchParams.filter.text;
        }
        return of({
            data: [
                {
                    id: 1,
                    name: 'first',
                    email: 'xxx',
                    firstName: 'First',
                    lastName: 'Last',
                    roles: [],
                    dateCreated: new Date().toISOString(),
                },
            ],
            pager: { total: 1, size: 1, index: 1 },
        });
    }

    removeItem(
        id: number
    ): Observable<HlcNzTable.Data.DataProviderResult<any>> {
        return of(null);
    }

    loadItem(id: number): Observable<User> {
        return of({
            id: 1,
            name: 'first',
            email: 'xxx',
            firstName: 'First',
            lastName: 'Last',
            roles: [],
            dateCreated: new Date().toISOString(),
        });
    }

    createItem(data: Partial<User>): Observable<User> {
        return of(data as any);
    }

    updateItem(id: number, data: Partial<User>): Observable<User> {
        return of(data as any);
    }
}
