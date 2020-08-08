import { ModuleWithProviders, NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { HttpErrorMessageInterceptor } from './http-error-message.interceptor';

@NgModule({
    declarations: [],
    imports: [CommonModule],
    providers: [
        {
            provide: HTTP_INTERCEPTORS,
            multi: true,
            useClass: HttpErrorMessageInterceptor,
        },
    ],
})
export class HttpErrorMessageInterceptorModule {}
