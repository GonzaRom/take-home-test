import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
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
}
