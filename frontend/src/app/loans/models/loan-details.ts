import { LoanSummary } from './loan-summary';

export interface PaymentDto {
  id: string;
  loanId: string;
  amount: number;
  paymentDateUtc: string;
  note: string | null;
}

export interface LoanDetails extends LoanSummary {
  payments: readonly PaymentDto[];
}
