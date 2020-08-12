import { HlcNzTable } from '@nz-holistic/nz-list';
import { ListDTO } from './list.dto';

export type MapItemFun = (item: any) => any;
export const mapListResponse = <T extends HlcNzTable.Row = HlcNzTable.Row>(
    mapItemFun: MapItemFun,
    _: HlcNzTable.Data.DataProviderState
) => (resultDTO: ListDTO.ListResponse): HlcNzTable.Data.DataProviderResult<T> => ({
    data: resultDTO.items.map(mapItemFun),
    pager: {
        index: resultDTO.pager.index,
        size: resultDTO.pager.size,
        total: resultDTO.pager.total
    }
});

export const mapListRequestParams = (request: HlcNzTable.Data.DataProviderState): ListDTO.ListRequestQueryParams => {
    const res = {
        index: request.pager.index.toString(),
        size: request.pager.size.toString()
    };

    const resSort = request.sort
        ? { ...res, sort: `${request.sort.order === 'ascend' ? '' : '-'}${request.sort.key}` }
        : res;

    return resSort;
};
