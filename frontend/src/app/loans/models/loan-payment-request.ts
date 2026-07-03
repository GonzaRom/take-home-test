export interface LoanPaymentRequest {
  amount: number;
  paymentDateUtc?: string;
  note?: string;
}
