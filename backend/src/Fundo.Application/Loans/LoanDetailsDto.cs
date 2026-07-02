namespace Fundo.Application.Loans;

public sealed class LoanDetailsDto : LoanSummaryDto
{
    public IReadOnlyList<PaymentDto> Payments { get; set; } = Array.Empty<PaymentDto>();
}
