using Fundo.Application.Common;
using Fundo.Application.Loans;
using Fundo.Domain.Loans;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Infrastructure.Persistence;

public sealed class LoanRepository : ILoanRepository
{
    private readonly LoanDbContext dbContext;

    public LoanRepository(LoanDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Loans
            .Include(loan => loan.Payments)
            .FirstOrDefaultAsync(loan => loan.Id == id, cancellationToken);
    }

    public async Task<IPagedResult<Loan>> GetListAsync(PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var totalCount = await dbContext.Loans.CountAsync(cancellationToken);
        var skip = (long)(pagination.PageNumber - 1) * pagination.PageSize;
        if (skip >= totalCount)
        {
            return new PagedResult<Loan>(Array.Empty<Loan>(), pagination.PageNumber, pagination.PageSize, totalCount);
        }

        var loans = await dbContext.Loans
            .OrderByDescending(loan => loan.CreatedAtUtc)
            .ThenBy(loan => loan.Id)
            .Skip((int)skip)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Loan>(loans, pagination.PageNumber, pagination.PageSize, totalCount);
    }

    public async Task AddAsync(Loan loan, CancellationToken cancellationToken = default)
    {
        await dbContext.Loans.AddAsync(loan, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
