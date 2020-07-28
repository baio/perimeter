import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'admin-signup-confirm-sent-page',
  templateUrl: './signup-confirm-sent-page.component.html',
  styleUrls: ['./signup-confirm-sent-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SignupConfirmSentPageComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

}
