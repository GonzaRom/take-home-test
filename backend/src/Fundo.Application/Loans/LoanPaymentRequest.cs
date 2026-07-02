using System.ComponentModel.DataAnnotations;

namespace Fundo.Application.Loans;

public sealed class LoanPaymentRequest : IValidatableObject
{
    public decimal Amount { get; set; }

    public DateTime? PaymentDateUtc { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Amount <= 0)
        {
            yield return new ValidationResult(
                "Payment amount must be greater than zero.",
                new[] { nameof(Amount) });
        }

        if (PaymentDateUtc.HasValue && PaymentDateUtc.Value.Kind != DateTimeKind.Utc)
        {
            yield return new ValidationResult(
                "Payment date must be in UTC.",
                new[] { nameof(PaymentDateUtc) });
        }
    }
}
