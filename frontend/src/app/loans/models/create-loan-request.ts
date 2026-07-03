export interface CreateLoanRequest {
  applicantName: string;
  applicantEmail: string;
  principalAmount: number;
  annualInterestRate: number;
  termMonths: number;
  currentBalance: number;
}
