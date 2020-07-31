import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoginPageComponent } from './login-page/login-page.component';
import { SignupPageComponent } from './signup-page/signup-page.component';
import { ForgotPasswordPageComponent } from './forgot-password-page/forgot-password-page.component';
import { ReactiveFormsModule } from '@angular/forms';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzCheckboxModule } from 'ng-zorro-antd/checkbox';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { RouterModule } from '@angular/router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AdminDataAccessModule } from '@admin/data-access';
import { SignupConfirmSentPageComponent } from './signup-confirm-sent-page/signup-confirm-sent-page.component';
import { SignupConfirmPageComponent } from './signup-confirm-page/signup-confirm-page.component';
import { ForgotPasswordSentPageComponent } from './forgot-password-sent-page/forgot-password-sent-page.component';

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
        AdminDataAccessModule,
    ],
    declarations: [
        LoginPageComponent,
        SignupPageComponent,
        ForgotPasswordPageComponent,
        SignupPageComponent,
        SignupConfirmSentPageComponent,
        SignupConfirmPageComponent,
        ForgotPasswordSentPageComponent,
    ],
    exports: [
        LoginPageComponent,
        SignupPageComponent,
        ForgotPasswordPageComponent,
        SignupPageComponent,
        SignupConfirmSentPageComponent,
        SignupConfirmPageComponent,
        ForgotPasswordSentPageComponent,
    ],
})
export class AdminAuthPagesModule {}
