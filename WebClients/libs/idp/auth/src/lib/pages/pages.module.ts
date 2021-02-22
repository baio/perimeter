import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';
import { AuthDataAccessModule } from '@idp/data-access';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzCheckboxModule } from 'ng-zorro-antd/checkbox';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { ForgotPasswordPageComponent } from './forgot-password-page/forgot-password-page.component';
import { ForgotPasswordResetPageComponent } from './forgot-password-reset-page/forgot-password-reset-page.component';
import { ForgotPasswordSentPageComponent } from './forgot-password-sent-page/forgot-password-sent-page.component';
import { LoginPageComponent } from './login-page/login-page.component';
import { PageHeaderComponent } from './page-header/page-header.component';
import { PreservedQueryParamsService } from './services';
import { SignupConfirmPageComponent } from './signup-confirm-page/signup-confirm-page.component';
import { SignupConfirmSentPageComponent } from './signup-confirm-sent-page/signup-confirm-sent-page.component';
import { SignupPageComponent } from './signup-page/signup-page.component';

@NgModule({
    imports: [
        CommonModule,
        ReactiveFormsModule,
        BrowserAnimationsModule,
        RouterModule,
        NzFormModule,
        NzInputModule,
        NzButtonModule,
        NzIconModule,
        NzCheckboxModule,
        NzSelectModule,
        NzModalModule,
        NzAlertModule,
        AuthDataAccessModule,
    ],
    declarations: [
        LoginPageComponent,
        SignupPageComponent,
        ForgotPasswordPageComponent,
        SignupPageComponent,
        SignupConfirmSentPageComponent,
        SignupConfirmPageComponent,
        ForgotPasswordSentPageComponent,
        ForgotPasswordResetPageComponent,
        PageHeaderComponent,
    ],
    exports: [
        LoginPageComponent,
        SignupPageComponent,
        ForgotPasswordPageComponent,
        SignupPageComponent,
        SignupConfirmSentPageComponent,
        SignupConfirmPageComponent,
        ForgotPasswordSentPageComponent,
        ForgotPasswordResetPageComponent,
    ],
    providers: [PreservedQueryParamsService],
})
export class IPPagesModule {}
