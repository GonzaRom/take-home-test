using Fundo.Domain.Loans;

namespace Fundo.Application.Loans;

public class LoanSummaryDto
{
    public Guid Id { get; set; }

    public string ApplicantName { get; set; } = string.Empty;

    public string ApplicantEmail { get; set; } = string.Empty;

    public decimal PrincipalAmount { get; set; }

    public decimal AnnualInterestRate { get; set; }

    public int TermMonths { get; set; }

    public decimal CurrentBalance { get; set; }

    public LoanStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
