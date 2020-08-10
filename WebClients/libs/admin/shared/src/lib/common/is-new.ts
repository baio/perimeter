import { ActivatedRoute } from '@angular/router';
import { map } from 'rxjs/operators';

export const isNew$ = (activatedRoute: ActivatedRoute) =>
    activatedRoute.params.pipe(map(params => params.id === 'new'));
