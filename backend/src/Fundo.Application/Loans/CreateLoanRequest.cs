using System.ComponentModel.DataAnnotations;

namespace Fundo.Application.Loans;

public sealed class CreateLoanRequest : IValidatableObject
{
    [Required(ErrorMessage = "Applicant name is required.")]
    [StringLength(200)]
    public string ApplicantName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Applicant email is required.")]
    [EmailAddress]
    [StringLength(320)]
    public string ApplicantEmail { get; set; } = string.Empty;

    public decimal PrincipalAmount { get; set; }

    public decimal AnnualInterestRate { get; set; }

    [Range(1, 360, ErrorMessage = "Term months must be between 1 and 360.")]
    public int TermMonths { get; set; }

    public decimal CurrentBalance { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PrincipalAmount <= 0)
        {
            yield return new ValidationResult(
                "Principal amount must be greater than zero.",
                new[] { nameof(PrincipalAmount) });
        }

        if (AnnualInterestRate < 0)
        {
            yield return new ValidationResult(
                "Annual interest rate cannot be negative.",
                new[] { nameof(AnnualInterestRate) });
        }

        if (CurrentBalance < 0)
        {
            yield return new ValidationResult(
                "Current balance must be between zero and the principal amount.",
                new[] { nameof(CurrentBalance) });
        }

        if (CurrentBalance > PrincipalAmount)
        {
            yield return new ValidationResult(
                "Current balance must be between zero and the principal amount.",
                new[] { nameof(CurrentBalance) });
        }
    }
}
