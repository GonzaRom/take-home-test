import { EventEmitter } from '@angular/core';
import { fakeAsync, tick } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { NEVER, of, Subject, throwError } from 'rxjs';

import { CreateLoanRequest } from '../../models/create-loan-request';
import { LoanDetails } from '../../models/loan-details';
import { LoanPaymentRequest } from '../../models/loan-payment-request';
import { LoanSummary } from '../../models/loan-summary';
import { PagedResult, PageRequest } from '../../models/pagination';
import { LoanApiService } from '../../services/loan-api.service';
import { LoanListContainerComponent } from './loan-list-container.component';

describe('LoanListContainerComponent', () => {
  let loanApiService: jasmine.SpyObj<LoanApiService>;
  let dialog: jasmine.SpyObj<MatDialog>;
  let snackBar: jasmine.SpyObj<MatSnackBar>;

  beforeEach(() => {
    loanApiService = jasmine.createSpyObj<LoanApiService>('LoanApiService', [
      'getLoans',
      'createLoan',
      'registerPayment',
    ]);
    dialog = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);
    snackBar = jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']);
  });

  it('exposes loading state while loans are loading', () => {
    const pendingLoans = new Subject<PagedResult<LoanSummary>>();
    const component = buildComponent();
    const viewModels: Array<{ isLoading: boolean; error: string | null }> = [];

    loanApiService.getLoans.and.returnValue(pendingLoans.asObservable());

    const subscription = component.vm$.subscribe((viewModel) => viewModels.push(viewModel));

    expect(viewModels[0]).toEqual(jasmine.objectContaining({
      isLoading: true,
      error: null,
    }));

    subscription.unsubscribe();
  });

  it('exposes an error state when the API fails', () => {
    const component = buildComponent();
    const viewModels: Array<{ isLoading: boolean; error: string | null }> = [];

    loanApiService.getLoans.and.returnValue(throwError(() => new Error('API unavailable')));

    const subscription = component.vm$.subscribe((viewModel) => viewModels.push(viewModel));
    const errorViewModel = viewModels[viewModels.length - 1];

    expect(errorViewModel).toEqual(jasmine.objectContaining({
      isLoading: false,
      error: 'Unable to load loans. Please confirm the backend API is running and try again.',
    }));

    subscription.unsubscribe();
  });

  it('refreshes loans after a successful create action', fakeAsync(() => {
    const createDialogComponent = {
      createRequested: new EventEmitter<CreateLoanRequest>(),
      startSubmit: jasmine.createSpy('startSubmit'),
      finishSubmitWithError: jasmine.createSpy('finishSubmitWithError'),
    };
    const dialogRef = {
      componentInstance: createDialogComponent,
      close: jasmine.createSpy('close'),
      afterClosed: () => NEVER,
    };
    const component = buildComponent();
    const request: CreateLoanRequest = {
      applicantName: 'Ada Lovelace',
      applicantEmail: 'ada@example.com',
      principalAmount: 10000,
      annualInterestRate: 6.5,
      termMonths: 36,
      currentBalance: 9500,
    };

    loanApiService.getLoans.and.callFake((pageRequest: PageRequest) => of(buildPagedLoans(pageRequest)));
    loanApiService.createLoan.and.returnValue(of(buildLoanDetails()));
    dialog.open.and.returnValue(dialogRef as any);

    const subscription = component.vm$.subscribe();

    component.openCreateLoanDialog();
    createDialogComponent.createRequested.emit(request);
    tick(300);

    expect(loanApiService.createLoan).toHaveBeenCalledOnceWith(request);
    expect(loanApiService.getLoans).toHaveBeenCalledTimes(2);
    expect(dialogRef.close).toHaveBeenCalledOnceWith(true);
    expect(snackBar.open).toHaveBeenCalledWith('Loan created successfully.', 'Dismiss', { duration: 4000 });

    subscription.unsubscribe();
  }));

  it('refreshes loans after a successful payment action', fakeAsync(() => {
    const paymentDialogComponent = {
      paymentRequested: new EventEmitter<LoanPaymentRequest>(),
      startSubmit: jasmine.createSpy('startSubmit'),
      finishSubmitWithError: jasmine.createSpy('finishSubmitWithError'),
    };
    const dialogRef = {
      componentInstance: paymentDialogComponent,
      close: jasmine.createSpy('close'),
      afterClosed: () => NEVER,
    };
    const component = buildComponent();
    const loan = buildLoanSummary({ id: 'loan-123', currentBalance: 500 });
    const request: LoanPaymentRequest = {
      amount: 100,
      note: 'Monthly payment',
    };

    loanApiService.getLoans.and.callFake((pageRequest: PageRequest) => of(buildPagedLoans(pageRequest)));
    loanApiService.registerPayment.and.returnValue(of(buildLoanDetails({ id: loan.id, currentBalance: 400 })));
    dialog.open.and.returnValue(dialogRef as any);

    const subscription = component.vm$.subscribe();

    component.openRegisterPaymentDialog(loan);
    paymentDialogComponent.paymentRequested.emit(request);
    tick(300);

    expect(loanApiService.registerPayment).toHaveBeenCalledOnceWith('loan-123', request);
    expect(loanApiService.getLoans).toHaveBeenCalledTimes(2);
    expect(dialogRef.close).toHaveBeenCalledOnceWith(true);
    expect(snackBar.open).toHaveBeenCalledWith('Payment registered successfully.', 'Dismiss', { duration: 4000 });

    subscription.unsubscribe();
  }));

  function buildComponent(): LoanListContainerComponent {
    return new LoanListContainerComponent(dialog, loanApiService, snackBar);
  }
});

function buildPagedLoans(pageRequest: PageRequest): PagedResult<LoanSummary> {
  return {
    items: [buildLoanSummary()],
    pageNumber: pageRequest.pageNumber,
    pageSize: pageRequest.pageSize,
    totalCount: 1,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
  };
}

function buildLoanSummary(overrides: Partial<LoanSummary> = {}): LoanSummary {
  return {
    id: 'loan-1',
    applicantName: 'Ada Lovelace',
    applicantEmail: 'ada@example.com',
    principalAmount: 10000,
    annualInterestRate: 6.5,
    termMonths: 36,
    currentBalance: 9500,
    status: 'Active',
    createdAtUtc: '2026-07-01T00:00:00Z',
    ...overrides,
  };
}

function buildLoanDetails(overrides: Partial<LoanDetails> = {}): LoanDetails {
  return {
    ...buildLoanSummary(),
    payments: [],
    ...overrides,
  };
}
