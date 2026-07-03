import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { CreateLoanRequest } from '../models/create-loan-request';
import { LoanDetails } from '../models/loan-details';
import { LoanPaymentRequest } from '../models/loan-payment-request';
import { LoanSummary } from '../models/loan-summary';
import { PagedResult, PageRequest } from '../models/pagination';

@Injectable({
  providedIn: 'root',
})
export class LoanApiService {
  private readonly loansUrl = `${environment.apiBaseUrl}/loans`;

  constructor(private readonly httpClient: HttpClient) {}

  getLoans({ pageNumber, pageSize }: PageRequest): Observable<PagedResult<LoanSummary>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.httpClient.get<PagedResult<LoanSummary>>(this.loansUrl, { params });
  }

  createLoan(request: CreateLoanRequest): Observable<LoanDetails> {
    return this.httpClient.post<LoanDetails>(this.loansUrl, request);
  }

  getLoan(id: string): Observable<LoanDetails> {
    return this.httpClient.get<LoanDetails>(`${this.loansUrl}/${id}`);
  }

  registerPayment(id: string, request: LoanPaymentRequest): Observable<LoanDetails> {
    return this.httpClient.post<LoanDetails>(`${this.loansUrl}/${id}/payment`, request);
  }
}
