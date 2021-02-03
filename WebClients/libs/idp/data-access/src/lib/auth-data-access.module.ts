import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { AuthDataAccessService } from './auth.data-access.service';

@NgModule({
    imports: [CommonModule, HttpClientModule],
    providers: [AuthDataAccessService],
})
export class AuthDataAccessModule {}
