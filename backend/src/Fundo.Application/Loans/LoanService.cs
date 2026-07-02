using Fundo.Application.Common;
using Fundo.Domain.Loans;

namespace Fundo.Application.Loans;

public sealed class LoanService : ILoanService
{
    private readonly ILoanRepository loanRepository;

    public LoanService(ILoanRepository loanRepository)
    {
        this.loanRepository = loanRepository;
    }

    public async Task<Result<LoanDetailsDto>> CreateAsync(CreateLoanRequest request, CancellationToken cancellationToken = default)
    {
        Loan loan;
        try
        {
            loan = Loan.Create(
                Guid.NewGuid(),
                request.ApplicantName,
                request.ApplicantEmail,
                request.PrincipalAmount,
                request.AnnualInterestRate,
                request.TermMonths,
                request.CurrentBalance,
                DateTime.UtcNow);
        }
        catch (LoanDomainException ex)
        {
            return Result<LoanDetailsDto>.Invalid(ex.Message);
        }

        await loanRepository.AddAsync(loan, cancellationToken);
        await loanRepository.SaveChangesAsync(cancellationToken);

        return Result<LoanDetailsDto>.Success(MapToDetailsDto(loan));
    }

    public async Task<Result<IReadOnlyList<LoanSummaryDto>>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var loans = await loanRepository.GetListAsync(cancellationToken);

        return Result<IReadOnlyList<LoanSummaryDto>>.Success(loans.Select(MapToSummaryDto).ToList());
    }

    public async Task<Result<LoanDetailsDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var loan = await loanRepository.GetByIdAsync(id, cancellationToken);

        return loan is null
            ? Result<LoanDetailsDto>.NotFound($"Loan {id} was not found.")
            : Result<LoanDetailsDto>.Success(MapToDetailsDto(loan));
    }

    public async Task<Result<LoanDetailsDto>> ApplyPaymentAsync(Guid id, LoanPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var loan = await loanRepository.GetByIdAsync(id, cancellationToken);
        if (loan is null)
        {
            return Result<LoanDetailsDto>.NotFound($"Loan {id} was not found.");
        }

        try
        {
            loan.RegisterPayment(request.Amount, request.PaymentDateUtc ?? DateTime.UtcNow, request.Note);
        }
        catch (LoanDomainException ex)
        {
            return Result<LoanDetailsDto>.Invalid(ex.Message);
        }

        await loanRepository.SaveChangesAsync(cancellationToken);

        return Result<LoanDetailsDto>.Success(MapToDetailsDto(loan));
    }

    private static LoanSummaryDto MapToSummaryDto(Loan loan)
    {
        return new LoanSummaryDto
        {
            Id = loan.Id,
            ApplicantName = loan.ApplicantName,
            ApplicantEmail = loan.ApplicantEmail,
            PrincipalAmount = loan.PrincipalAmount,
            AnnualInterestRate = loan.AnnualInterestRate,
            TermMonths = loan.TermMonths,
            CurrentBalance = loan.CurrentBalance,
            Status = loan.Status,
            CreatedAtUtc = loan.CreatedAtUtc
        };
    }

    private static LoanDetailsDto MapToDetailsDto(Loan loan)
    {
        return new LoanDetailsDto
        {
            Id = loan.Id,
            ApplicantName = loan.ApplicantName,
            ApplicantEmail = loan.ApplicantEmail,
            PrincipalAmount = loan.PrincipalAmount,
            AnnualInterestRate = loan.AnnualInterestRate,
            TermMonths = loan.TermMonths,
            CurrentBalance = loan.CurrentBalance,
            Status = loan.Status,
            CreatedAtUtc = loan.CreatedAtUtc,
            Payments = loan.Payments
                .OrderBy(payment => payment.PaymentDateUtc)
                .Select(MapToDto)
                .ToList()
        };
    }

    private static PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            LoanId = payment.LoanId,
            Amount = payment.Amount,
            PaymentDateUtc = payment.PaymentDateUtc,
            Note = payment.Note
        };
    }
}
