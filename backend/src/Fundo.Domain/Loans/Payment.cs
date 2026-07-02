namespace Fundo.Domain.Loans;

public sealed class Payment
{
    private Payment()
    {
        Note = string.Empty;
    }

    internal Payment(Guid id, Guid loanId, decimal amount, DateTime paymentDateUtc, string? note)
    {
        if (id == Guid.Empty)
        {
            throw new LoanDomainException("Payment id is required.");
        }

        if (loanId == Guid.Empty)
        {
            throw new LoanDomainException("Loan id is required.");
        }

        if (amount <= 0)
        {
            throw new LoanDomainException("Payment amount must be greater than zero.");
        }

        if (paymentDateUtc.Kind != DateTimeKind.Utc)
        {
            throw new LoanDomainException("Payment date must be in UTC.");
        }

        Id = id;
        LoanId = loanId;
        Amount = amount;
        PaymentDateUtc = paymentDateUtc;
        Note = note?.Trim();
    }

    public Guid Id { get; private set; }

    public Guid LoanId { get; private set; }

    public decimal Amount { get; private set; }

    public DateTime PaymentDateUtc { get; private set; }

    public string? Note { get; private set; }
}
