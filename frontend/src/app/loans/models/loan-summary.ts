export type LoanStatus = 1 | 2 | 'Active' | 'Paid' | 'paid' | 'PaidOff' | 'paidOff';

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

export function isPaidLoanStatus(status: LoanStatus): boolean {
  if (status === 2) {
    return true;
  }

  if (typeof status === 'number') {
    return false;
  }

  const normalizedStatus = status.toLowerCase().replace(/\s/g, '');
  return normalizedStatus === 'paid' || normalizedStatus === 'paidoff';
}

export function formatLoanStatus(status: LoanStatus): string {
  if (status === 1) {
    return 'Active';
  }

  if (isPaidLoanStatus(status)) {
    return 'Paid Off';
  }

  if (typeof status === 'number') {
    return status.toString();
  }

  return status.replace(/([a-z])([A-Z])/g, '$1 $2');
}
