import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'data-avail-signup-page',
  templateUrl: './signup-page.component.html',
  styleUrls: ['./signup-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SignupPageComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

}
