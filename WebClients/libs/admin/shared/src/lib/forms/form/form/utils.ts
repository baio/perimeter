import { Observable, concat } from 'rxjs';
import { delay, filter, take } from 'rxjs/operators';

export const delayWhile = <T>(status$: Observable<T>, checker: () => boolean): Observable<T> => {
    //
    const valueWithStatus1$ = status$.pipe(delay(0), filter(checker), take(1));
    // Regular status change
    const valueWithStatus2$ = status$.pipe(filter(checker));
    // Combine both
    return concat(valueWithStatus1$, valueWithStatus2$);
};
