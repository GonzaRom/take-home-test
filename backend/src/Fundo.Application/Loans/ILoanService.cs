using Fundo.Application.Common;

namespace Fundo.Application.Loans;

public interface ILoanService
{
    Task<Result<LoanDetailsDto>> CreateAsync(CreateLoanRequest request, CancellationToken cancellationToken = default);

    Task<Result<IPagedResult<LoanSummaryDto>>> GetListAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);

    Task<Result<LoanDetailsDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<LoanDetailsDto>> ApplyPaymentAsync(Guid id, LoanPaymentRequest request, CancellationToken cancellationToken = default);
}
