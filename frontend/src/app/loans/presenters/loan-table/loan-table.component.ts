import { CurrencyPipe, TitleCasePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';

import { LoanStatus, LoanSummary } from '../../models/loan-summary';
import { PageRequest } from '../../models/pagination';

@Component({
  selector: 'app-loan-table',
  standalone: true,
  imports: [
    CurrencyPipe,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatPaginatorModule,
    MatTableModule,
    TitleCasePipe,
  ],
  templateUrl: './loan-table.component.html',
  styleUrl: './loan-table.component.scss',
})
export class LoanTableComponent {
  @Input({ required: true }) loans: readonly LoanSummary[] = [];
  @Input({ required: true }) totalCount = 0;
  @Input({ required: true }) pageNumber = 1;
  @Input({ required: true }) pageSize = 10;
  @Input() isEmpty = false;
  @Output() readonly pageChanged = new EventEmitter<PageRequest>();
  @Output() readonly refreshRequested = new EventEmitter<void>();

  readonly pageSizeOptions = [5, 10, 25, 50];
  readonly displayedColumns = [
    'principalAmount',
    'currentBalance',
    'applicantName',
    'status',
  ];

  refresh(): void {
    this.refreshRequested.emit();
  }

  onPageChange(event: PageEvent): void {
    this.pageChanged.emit({
      pageNumber: event.pageIndex + 1,
      pageSize: event.pageSize,
    });
  }

  getStatusLabel(status: LoanStatus): string {
    if (status === 1) {
      return 'Active';
    }

    if (status === 2) {
      return 'Paid Off';
    }

    return status.replace(/([a-z])([A-Z])/g, '$1 $2');
  }

  isPaidStatus(status: LoanStatus): boolean {
    return status === 2 || status === 'PaidOff';
  }

  trackByLoanId(_index: number, loan: LoanSummary): string {
    return loan.id;
  }
}
