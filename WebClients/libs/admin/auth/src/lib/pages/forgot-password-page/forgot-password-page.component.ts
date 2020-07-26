import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'data-avail-forgot-password-page',
  templateUrl: './forgot-password-page.component.html',
  styleUrls: ['./forgot-password-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ForgotPasswordPageComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

}
