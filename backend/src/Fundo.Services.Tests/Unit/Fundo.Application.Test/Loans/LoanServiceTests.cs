using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Fundo.Application.Common;
using Fundo.Application.Loans;
using Fundo.Domain.Loans;
using Moq;
using Xunit;

namespace Fundo.Services.Tests.Unit.Fundo.Application.Test.Loans;

public sealed class LoanServiceTests
{
    [Fact]
    public async Task GetListAsync_ShouldReturnLoanSummaries_WhenLoansHavePaymentHistory()
    {
        // Arrange
        var loan = Loan.Create(
            Guid.Parse("c2f4ebf4-fd56-4a46-8700-45ff59abafab"),
            "Ada Lovelace",
            "ada@example.com",
            1000m,
            12.5m,
            24,
            1000m,
            new DateTime(2026, 7, 2, 12, 0, 0, DateTimeKind.Utc));
        loan.RegisterPayment(125m, new DateTime(2026, 7, 3, 12, 0, 0, DateTimeKind.Utc), "first payment");

        var loanRepository = new Mock<ILoanRepository>();
        loanRepository
            .Setup(repository => repository.GetListAsync(It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Loan>(new List<Loan> { loan }, 1, 10, 1));

        var service = new LoanService(loanRepository.Object);

        // Act
        var result = await service.GetListAsync(new PaginationRequest());

        // Assert
        result.Status.Should().Be(ResultStatus.Success);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalCount.Should().Be(1);
        var summary = result.Value.Items.Should().ContainSingle().Subject;
        summary.Id.Should().Be(loan.Id);
        summary.ApplicantName.Should().Be(loan.ApplicantName);
        summary.ApplicantEmail.Should().Be(loan.ApplicantEmail);
        summary.CurrentBalance.Should().Be(875m);
        typeof(LoanSummaryDto).GetProperty("Payments").Should().BeNull();
    }
}
