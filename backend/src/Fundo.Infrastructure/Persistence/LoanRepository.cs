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

    public async Task<IReadOnlyList<Loan>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Loans
            .OrderBy(loan => loan.ApplicantName)
            .ToListAsync(cancellationToken);
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
