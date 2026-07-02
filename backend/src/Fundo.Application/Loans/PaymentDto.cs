namespace Fundo.Application.Loans;

public sealed class PaymentDto
{
    public Guid Id { get; set; }

    public Guid LoanId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDateUtc { get; set; }

    public string? Note { get; set; }
}
