import { AsyncPipe } from '@angular/common';
import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BehaviorSubject, EMPTY, merge, Observable, of, Subject } from 'rxjs';
import { catchError, debounceTime, exhaustMap, map, shareReplay, startWith, switchMap, takeUntil, tap } from 'rxjs/operators';

import { CreateLoanDialogComponent } from '../../dialogs/create-loan-dialog/create-loan-dialog.component';
import { LoanDetailsDialogComponent } from '../../dialogs/loan-details-dialog/loan-details-dialog.component';
import { RegisterPaymentDialogComponent } from '../../dialogs/register-payment-dialog/register-payment-dialog.component';
import { isPaidLoanStatus, LoanSummary } from '../../models/loan-summary';
import { PagedResult, PageRequest } from '../../models/pagination';
import { extractApiErrorMessage } from '../../services/api-error-message';
import { LoanApiService } from '../../services/loan-api.service';
import { LoanTableComponent } from '../../presenters/loan-table/loan-table.component';

interface LoanListViewModel {
  readonly loans: readonly LoanSummary[];
  readonly totalCount: number;
  readonly pageNumber: number;
  readonly pageSize: number;
  readonly isLoading: boolean;
  readonly isEmpty: boolean;
  readonly error: string | null;
}

@Component({
  selector: 'app-loan-list-container',
  standalone: true,
  imports: [
    AsyncPipe,
    LoanTableComponent,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './loan-list-container.component.html',
  styleUrl: './loan-list-container.component.scss',
})
export class LoanListContainerComponent {
  private readonly initialPageRequest: PageRequest = {
    pageNumber: 1,
    pageSize: 10,
  };

  private readonly pageRequestTrigger = new BehaviorSubject<PageRequest>(this.initialPageRequest);
  private readonly refreshTrigger = new Subject<void>();
  private readonly refreshDebounceMs = 300;

  readonly vm$: Observable<LoanListViewModel> = merge(
    this.pageRequestTrigger,
    this.refreshTrigger.pipe(
      debounceTime(this.refreshDebounceMs),
      map(() => this.pageRequestTrigger.value),
    ),
  ).pipe(
    switchMap((pageRequest) =>
      this.loanApiService.getLoans(pageRequest).pipe(
        map((response) => this.loadedViewModel(response)),
        startWith(this.loadingViewModel(pageRequest)),
        catchError(() => of(this.errorViewModel(pageRequest))),
      ),
    ),
    shareReplay({ bufferSize: 1, refCount: true }),
  );

  constructor(
    private readonly dialog: MatDialog,
    private readonly loanApiService: LoanApiService,
    private readonly snackBar: MatSnackBar,
  ) {}

  refresh(): void {
    this.refreshTrigger.next();
  }

  changePage(pageRequest: PageRequest): void {
    this.pageRequestTrigger.next(pageRequest);
  }

  openCreateLoanDialog(): void {
    const dialogRef = this.dialog.open(CreateLoanDialogComponent, {
      width: '620px',
      maxWidth: 'calc(100vw - 24px)',
      autoFocus: 'first-tabbable',
    });

    dialogRef.componentInstance.createRequested
      .pipe(
        exhaustMap((request) => {
          dialogRef.componentInstance.startSubmit();

          return this.loanApiService.createLoan(request).pipe(
            tap(() => {
              this.showSuccess('Loan created successfully.');
              dialogRef.close(true);
              this.refresh();
            }),
            catchError((error: unknown) => {
              dialogRef.componentInstance.finishSubmitWithError(
                extractApiErrorMessage(error, 'Unable to create loan. Please review the form and try again.'),
              );
              return EMPTY;
            }),
          );
        }),
        takeUntil(dialogRef.afterClosed()),
      )
      .subscribe();
  }

  openLoanDetailsDialog(loan: LoanSummary): void {
    this.dialog.open(LoanDetailsDialogComponent, {
      width: '760px',
      maxWidth: 'calc(100vw - 24px)',
      autoFocus: 'dialog',
      data: { loanId: loan.id },
    });
  }

  openRegisterPaymentDialog(loan: LoanSummary): void {
    if (!this.canRegisterPayment(loan)) {
      return;
    }

    const dialogRef = this.dialog.open(RegisterPaymentDialogComponent, {
      width: '540px',
      maxWidth: 'calc(100vw - 24px)',
      autoFocus: 'first-tabbable',
      data: { loan },
    });

    dialogRef.componentInstance.paymentRequested
      .pipe(
        exhaustMap((request) => {
          dialogRef.componentInstance.startSubmit();

          return this.loanApiService.registerPayment(loan.id, request).pipe(
            tap(() => {
              this.showSuccess('Payment registered successfully.');
              dialogRef.close(true);
              this.refresh();
            }),
            catchError((error: unknown) => {
              dialogRef.componentInstance.finishSubmitWithError(
                extractApiErrorMessage(error, 'Unable to register payment. Please review the form and try again.'),
              );
              return EMPTY;
            }),
          );
        }),
        takeUntil(dialogRef.afterClosed()),
      )
      .subscribe();
  }

  private loadedViewModel(response: PagedResult<LoanSummary>): LoanListViewModel {
    return {
      loans: response.items,
      totalCount: response.totalCount,
      pageNumber: response.pageNumber,
      pageSize: response.pageSize,
      isLoading: false,
      isEmpty: response.totalCount === 0,
      error: null,
    };
  }

  private loadingViewModel(pageRequest: PageRequest): LoanListViewModel {
    return {
      loans: [],
      totalCount: 0,
      pageNumber: pageRequest.pageNumber,
      pageSize: pageRequest.pageSize,
      isLoading: true,
      isEmpty: false,
      error: null,
    };
  }

  private errorViewModel(pageRequest: PageRequest): LoanListViewModel {
    return {
      loans: [],
      totalCount: 0,
      pageNumber: pageRequest.pageNumber,
      pageSize: pageRequest.pageSize,
      isLoading: false,
      isEmpty: false,
      error: 'Unable to load loans. Please confirm the backend API is running and try again.',
    };
  }

  private canRegisterPayment(loan: LoanSummary): boolean {
    return !isPaidLoanStatus(loan.status) && loan.currentBalance > 0;
  }

  private showSuccess(message: string): void {
    this.snackBar.open(message, 'Dismiss', {
      duration: 4000,
    });
  }
}
