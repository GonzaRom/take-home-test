import { CurrencyPipe, TitleCasePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';

import { formatLoanStatus, isPaidLoanStatus, LoanStatus, LoanSummary } from '../../models/loan-summary';
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
    MatTooltipModule,
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
  @Output() readonly addLoanRequested = new EventEmitter<void>();
  @Output() readonly pageChanged = new EventEmitter<PageRequest>();
  @Output() readonly refreshRequested = new EventEmitter<void>();
  @Output() readonly viewDetailsRequested = new EventEmitter<LoanSummary>();
  @Output() readonly registerPaymentRequested = new EventEmitter<LoanSummary>();

  readonly pageSizeOptions = [5, 10, 25, 50];
  readonly displayedColumns = [
    'principalAmount',
    'currentBalance',
    'applicantName',
    'status',
    'actions',
  ];

  addLoan(): void {
    this.addLoanRequested.emit();
  }

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
    return formatLoanStatus(status);
  }

  isPaidStatus(status: LoanStatus): boolean {
    return isPaidLoanStatus(status);
  }

  canRegisterPayment(loan: LoanSummary): boolean {
    return !this.isPaidStatus(loan.status) && loan.currentBalance > 0;
  }

  getPaymentDisabledTooltip(loan: LoanSummary): string {
    if (this.isPaidStatus(loan.status)) {
      return 'This loan is already paid off.';
    }

    if (loan.currentBalance <= 0) {
      return 'This loan has no remaining balance.';
    }

    return '';
  }

  viewDetails(loan: LoanSummary, event: MouseEvent): void {
    event.stopPropagation();
    this.viewDetailsRequested.emit(loan);
  }

  registerPayment(loan: LoanSummary, event: MouseEvent): void {
    event.stopPropagation();

    if (this.canRegisterPayment(loan)) {
      this.registerPaymentRequested.emit(loan);
    }
  }

  trackByLoanId(_index: number, loan: LoanSummary): string {
    return loan.id;
  }
}
