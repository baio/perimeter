import { Injectable } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

const preservedParams = [
    'client_id',
    'redirect_uri',
    'scope',
    'state',
    'response_type',
    'code_challenge',
    'code_challenge_method',
];

export type AuthParamName =
    | 'client_id'
    | 'redirect_uri'
    | 'scope'
    | 'state'
    | 'response_type'
    | 'code_challenge'
    | 'code_challenge_method';

@Injectable()
export class PreservedQueryParamsService {
    constructor(private readonly activatedRoute: ActivatedRoute) {}

    getAuthParam(key: AuthParamName) {
        return this.activatedRoute.snapshot.queryParamMap.get(key);
    }

    getAuthParams() {
        const queryParams = this.activatedRoute.snapshot.queryParamMap;
        const res = preservedParams.reduce((acc, k) => {
            const v = queryParams.get(k);
            if (v) {
                return { ...acc, [k]: v };
            } else {
                return acc;
            }
        }, {});
        return res;
    }
}
