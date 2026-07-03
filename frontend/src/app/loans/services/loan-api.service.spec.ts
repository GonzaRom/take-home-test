import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { CreateLoanRequest } from '../models/create-loan-request';
import { LoanDetails } from '../models/loan-details';
import { LoanPaymentRequest } from '../models/loan-payment-request';
import { LoanSummary } from '../models/loan-summary';
import { PagedResult } from '../models/pagination';
import { LoanApiService } from './loan-api.service';

describe('LoanApiService', () => {
  let service: LoanApiService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        LoanApiService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(LoanApiService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('gets the loans list from the expected endpoint with paging', () => {
    const response: PagedResult<LoanSummary> = {
      items: [],
      pageNumber: 2,
      pageSize: 25,
      totalCount: 0,
      totalPages: 0,
      hasPreviousPage: true,
      hasNextPage: false,
    };

    service.getLoans({ pageNumber: 2, pageSize: 25 }).subscribe((result) => {
      expect(result).toEqual(response);
    });

    const request = httpTestingController.expectOne((req) =>
      req.method === 'GET' && req.url === `${environment.apiBaseUrl}/loans`,
    );

    expect(request.request.params.get('pageNumber')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('25');

    request.flush(response);
  });

  it('gets loan details from the expected endpoint with id', () => {
    const response = buildLoanDetails({ id: 'loan-123' });

    service.getLoan('loan-123').subscribe((result) => {
      expect(result).toEqual(response);
    });

    const request = httpTestingController.expectOne(`${environment.apiBaseUrl}/loans/loan-123`);

    expect(request.request.method).toBe('GET');

    request.flush(response);
  });

  it('posts create loan requests to the expected endpoint with payload', () => {
    const payload: CreateLoanRequest = {
      applicantName: 'Ada Lovelace',
      applicantEmail: 'ada@example.com',
      principalAmount: 10000,
      annualInterestRate: 6.5,
      termMonths: 36,
      currentBalance: 9500,
    };
    const response = buildLoanDetails();

    service.createLoan(payload).subscribe((result) => {
      expect(result).toEqual(response);
    });

    const request = httpTestingController.expectOne(`${environment.apiBaseUrl}/loans`);

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);

    request.flush(response);
  });

  it('posts payment requests to the expected endpoint with payload', () => {
    const payload: LoanPaymentRequest = {
      amount: 250,
      note: 'Extra payment',
    };
    const response = buildLoanDetails({ id: 'loan-123', currentBalance: 7250 });

    service.registerPayment('loan-123', payload).subscribe((result) => {
      expect(result).toEqual(response);
    });

    const request = httpTestingController.expectOne(`${environment.apiBaseUrl}/loans/loan-123/payment`);

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);

    request.flush(response);
  });
});

function buildLoanDetails(overrides: Partial<LoanDetails> = {}): LoanDetails {
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
    payments: [],
    ...overrides,
  };
}
