import { Component } from '@angular/core';

import { LoanListContainerComponent } from './loans/containers/loan-list-container/loan-list-container.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [LoanListContainerComponent],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {}
