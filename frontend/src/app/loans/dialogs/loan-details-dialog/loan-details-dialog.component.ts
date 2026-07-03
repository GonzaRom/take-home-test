import { AsyncPipe, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { Observable, of } from 'rxjs';
import { catchError, map, startWith } from 'rxjs/operators';

import { LoanDetails } from '../../models/loan-details';
import { formatLoanStatus } from '../../models/loan-summary';
import { extractApiErrorMessage } from '../../services/api-error-message';
import { LoanApiService } from '../../services/loan-api.service';

export interface LoanDetailsDialogData {
  readonly loanId: string;
}

interface LoanDetailsViewModel {
  readonly loan: LoanDetails | null;
  readonly isLoading: boolean;
  readonly error: string | null;
}

@Component({
  selector: 'app-loan-details-dialog',
  standalone: true,
  imports: [
    AsyncPipe,
    CurrencyPipe,
    DatePipe,
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
  ],
  templateUrl: './loan-details-dialog.component.html',
  styleUrl: './loan-details-dialog.component.scss',
})
export class LoanDetailsDialogComponent {
  private readonly data = inject<LoanDetailsDialogData>(MAT_DIALOG_DATA);
  private readonly loanApiService = inject(LoanApiService);

  readonly paymentColumns = ['paymentDateUtc', 'amount', 'note'];

  readonly vm$: Observable<LoanDetailsViewModel> = this.loanApiService.getLoan(this.data.loanId).pipe(
    map((loan) => ({
      loan,
      isLoading: false,
      error: null,
    })),
    startWith({
      loan: null,
      isLoading: true,
      error: null,
    }),
    catchError((error: unknown) =>
      of({
        loan: null,
        isLoading: false,
        error: extractApiErrorMessage(error, 'Unable to load loan details. Please try again.'),
      }),
    ),
  );

  getStatusLabel(loan: LoanDetails): string {
    return formatLoanStatus(loan.status);
  }

  toBrowserLocalDate(utcDate: string): Date {
    const hasTimezone = /(?:z|[+-]\d{2}:?\d{2})$/i.test(utcDate);
    return new Date(hasTimezone ? utcDate : `${utcDate}Z`);
  }
}
