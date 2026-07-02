namespace Fundo.Domain.Loans;

public class Loan
{
    public const int MaximumTermMonths = 360;

    private readonly List<Payment> payments = new();

    protected Loan()
    {
        ApplicantName = string.Empty;
        ApplicantEmail = string.Empty;
    }

    private Loan(
        Guid id,
        string applicantName,
        string applicantEmail,
        decimal principalAmount,
        decimal annualInterestRate,
        int termMonths,
        decimal currentBalance,
        DateTime createdAtUtc)
    {
        ValidateLoan(id, applicantName, applicantEmail, principalAmount, annualInterestRate, termMonths, currentBalance, createdAtUtc);

        Id = id;
        ApplicantName = applicantName.Trim();
        ApplicantEmail = applicantEmail.Trim();
        PrincipalAmount = principalAmount;
        AnnualInterestRate = annualInterestRate;
        TermMonths = termMonths;
        CurrentBalance = currentBalance;
        Status = GetStatusForBalance(currentBalance);
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public string ApplicantName { get; private set; }

    public string ApplicantEmail { get; private set; }

    public decimal PrincipalAmount { get; private set; }

    public decimal AnnualInterestRate { get; private set; }

    public int TermMonths { get; private set; }

    public decimal CurrentBalance { get; private set; }

    public LoanStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<Payment> Payments => payments.AsReadOnly();

    public static Loan Create(
        Guid id,
        string applicantName,
        string applicantEmail,
        decimal principalAmount,
        decimal annualInterestRate,
        int termMonths,
        decimal? currentBalance = null,
        DateTime? createdAtUtc = null)
    {
        return new Loan(
            id,
            applicantName,
            applicantEmail,
            principalAmount,
            annualInterestRate,
            termMonths,
            currentBalance ?? principalAmount,
            createdAtUtc ?? DateTime.UtcNow);
    }

    public Payment RegisterPayment(decimal amount, DateTime paymentDateUtc, string? note = null)
    {
        EnsureCanRegisterPayment(amount);
        var payment = new Payment(Guid.NewGuid(), Id, amount, paymentDateUtc, note);

        // MVP: payments reduce the outstanding balance directly; production lending would need amortization and concurrency controls.
        CurrentBalance -= amount;
        Status = GetStatusForBalance(CurrentBalance);
        payments.Add(payment);

        return payment;
    }

    private static void ValidateLoan(
        Guid id,
        string applicantName,
        string applicantEmail,
        decimal principalAmount,
        decimal annualInterestRate,
        int termMonths,
        decimal currentBalance,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new LoanDomainException("Loan id is required.");
        }

        if (string.IsNullOrWhiteSpace(applicantName))
        {
            throw new LoanDomainException("Applicant name is required.");
        }

        if (string.IsNullOrWhiteSpace(applicantEmail))
        {
            throw new LoanDomainException("Applicant email is required.");
        }

        if (principalAmount <= 0)
        {
            throw new LoanDomainException("Principal amount must be greater than zero.");
        }

        if (annualInterestRate < 0)
        {
            throw new LoanDomainException("Annual interest rate cannot be negative.");
        }

        if (termMonths <= 0 || termMonths > MaximumTermMonths)
        {
            throw new LoanDomainException($"Term months must be between 1 and {MaximumTermMonths}.");
        }

        if (currentBalance < 0 || currentBalance > principalAmount)
        {
            throw new LoanDomainException("Current balance must be between zero and the principal amount.");
        }

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new LoanDomainException("Loan creation date must be in UTC.");
        }
    }

    private void EnsureCanRegisterPayment(decimal amount)
    {
        if (amount <= 0)
        {
            throw new LoanDomainException("Payment amount must be greater than zero.");
        }

        if (Status == LoanStatus.PaidOff)
        {
            throw new LoanDomainException("Paid off loans cannot receive additional payments.");
        }

        if (amount > CurrentBalance)
        {
            throw new LoanDomainException("Payment amount cannot exceed current balance.");
        }
    }

    private static LoanStatus GetStatusForBalance(decimal currentBalance)
    {
        return currentBalance == 0 ? LoanStatus.PaidOff : LoanStatus.Active;
    }
}
