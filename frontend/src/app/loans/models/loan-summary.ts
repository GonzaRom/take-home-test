export type LoanStatus = 1 | 2 | 'Active' | 'PaidOff';

export interface LoanSummary {
  id: string;
  applicantName: string;
  applicantEmail: string;
  principalAmount: number;
  annualInterestRate: number;
  termMonths: number;
  currentBalance: number;
  status: LoanStatus;
  createdAtUtc: string;
}
