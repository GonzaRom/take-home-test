using Fundo.Application.Common;
using Fundo.Domain.Loans;

namespace Fundo.Application.Loans;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IPagedResult<Loan>> GetListAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);

    Task AddAsync(Loan loan, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
