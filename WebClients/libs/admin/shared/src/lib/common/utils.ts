import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export const not$ = (observable: Observable<any>) =>
    observable.pipe(map((x) => !x));
