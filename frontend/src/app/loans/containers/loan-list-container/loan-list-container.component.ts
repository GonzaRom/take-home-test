import { AsyncPipe } from '@angular/common';
import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BehaviorSubject, merge, Observable, of, Subject } from 'rxjs';
import { catchError, debounceTime, map, shareReplay, startWith, switchMap } from 'rxjs/operators';

import { LoanSummary } from '../../models/loan-summary';
import { PagedResult, PageRequest } from '../../models/pagination';
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

  constructor(private readonly loanApiService: LoanApiService) {}

  refresh(): void {
    this.refreshTrigger.next();
  }

  changePage(pageRequest: PageRequest): void {
    this.pageRequestTrigger.next(pageRequest);
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
}
