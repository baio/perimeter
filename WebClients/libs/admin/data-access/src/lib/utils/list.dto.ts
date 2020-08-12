export namespace ListDTO {
    export interface ListRequestQueryParams {
        index: string;
        size: string;
        sort?: string;
        filter?: any;
        [param: string]: string | string[];
    }

    export interface ListResponse<T = { id: number } & any> {
        items: T[];
        pager: { total: number; size: number; index: number };
    }
}
