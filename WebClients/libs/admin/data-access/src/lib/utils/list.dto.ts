export namespace ListDTO {
    export interface ListRequestQueryParams {
        page: string;
        limit: string;
        sort?: string;
        filter?: any;
        [param: string]: string | string[];
    }

    export interface ListResponse<T = { id: number } & any> {
        items: T[];
        pager: { length: number; limit: number; page: number };
    }
}
