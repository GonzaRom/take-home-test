using System;
using FluentAssertions;
using Fundo.Domain.Loans;
using Xunit;

namespace Fundo.Services.Tests.Unit.Fundo.Domain.Test.Loans;

public sealed class LoanTests
{
    private static readonly Guid LoanId = Guid.Parse("9d262493-692c-4cfe-8f11-f925a4bb3760");
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 2, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime PaymentDateUtc = new(2026, 7, 3, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WithValidValues_ShouldCreateActiveLoan()
    {
        var loan = CreateLoan(applicantName: "  Ada Lovelace  ");

        loan.Id.Should().Be(LoanId);
        loan.ApplicantName.Should().Be("Ada Lovelace");
        loan.ApplicantEmail.Should().Be("ada@example.com");
        loan.PrincipalAmount.Should().Be(1000m);
        loan.AnnualInterestRate.Should().Be(12.5m);
        loan.TermMonths.Should().Be(24);
        loan.CurrentBalance.Should().Be(1000m);
        loan.Status.Should().Be(LoanStatus.Active);
        loan.CreatedAtUtc.Should().Be(CreatedAtUtc);
        loan.Payments.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithZeroBalance_ShouldCreatePaidOffLoan()
    {
        var loan = CreateLoan(currentBalance: 0m);

        loan.Status.Should().Be(LoanStatus.PaidOff);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutApplicantName_ShouldRejectLoan(string applicantName)
    {
        Action action = () => CreateLoan(applicantName: applicantName);

        action.Should().Throw<LoanDomainException>()
            .WithMessage("Applicant name is required.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidPrincipalAmount_ShouldRejectLoan(decimal principalAmount)
    {
        Action action = () => CreateLoan(principalAmount: principalAmount);

        action.Should().Throw<LoanDomainException>()
            .WithMessage("Principal amount must be greater than zero.");
    }

    [Fact]
    public void Create_WithNegativeAnnualInterestRate_ShouldRejectLoan()
    {
        Action action = () => CreateLoan(annualInterestRate: -0.1m);

        action.Should().Throw<LoanDomainException>()
            .WithMessage("Annual interest rate cannot be negative.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(361)]
    public void Create_WithInvalidTermMonths_ShouldRejectLoan(int termMonths)
    {
        Action action = () => CreateLoan(termMonths: termMonths);

        action.Should().Throw<LoanDomainException>()
            .WithMessage("Term months must be between 1 and 360.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1000.01)]
    public void Create_WithInvalidCurrentBalance_ShouldRejectLoan(decimal currentBalance)
    {
        Action action = () => CreateLoan(currentBalance: currentBalance);

        action.Should().Throw<LoanDomainException>()
            .WithMessage("Current balance must be between zero and the principal amount.");
    }

    [Fact]
    public void RegisterPayment_WithValidAmount_ShouldReduceBalanceAndRecordPayment()
    {
        var loan = CreateLoan(currentBalance: 1000m);

        var payment = loan.RegisterPayment(250m, PaymentDateUtc, "  first payment  ");

        loan.CurrentBalance.Should().Be(750m);
        loan.Status.Should().Be(LoanStatus.Active);
        loan.Payments.Should().ContainSingle().Which.Should().Be(payment);
        payment.LoanId.Should().Be(loan.Id);
        payment.Amount.Should().Be(250m);
        payment.PaymentDateUtc.Should().Be(PaymentDateUtc);
        payment.Note.Should().Be("first payment");
    }

    [Fact]
    public void RegisterPayment_WithExactPayoff_ShouldMarkLoanPaidOff()
    {
        var loan = CreateLoan(currentBalance: 1000m);

        loan.RegisterPayment(1000m, PaymentDateUtc);

        loan.CurrentBalance.Should().Be(0m);
        loan.Status.Should().Be(LoanStatus.PaidOff);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RegisterPayment_WithInvalidAmount_ShouldRejectPayment(decimal amount)
    {
        var loan = CreateLoan();

        Action action = () => loan.RegisterPayment(amount, PaymentDateUtc);

        action.Should().Throw<LoanDomainException>()
            .WithMessage("Payment amount must be greater than zero.");
    }

    [Fact]
    public void RegisterPayment_WhenAmountExceedsBalance_ShouldRejectPayment()
    {
        var loan = CreateLoan(currentBalance: 100m);

        Action action = () => loan.RegisterPayment(100.01m, PaymentDateUtc);

        action.Should().Throw<LoanDomainException>()
            .WithMessage("Payment amount cannot exceed current balance.");
    }

    [Fact]
    public void RegisterPayment_WhenLoanIsPaidOff_ShouldRejectPayment()
    {
        var loan = CreateLoan(currentBalance: 0m);

        Action action = () => loan.RegisterPayment(1m, PaymentDateUtc);

        action.Should().Throw<LoanDomainException>()
            .WithMessage("Paid off loans cannot receive additional payments.");
    }

    private static Loan CreateLoan(
        string applicantName = "Ada Lovelace",
        string applicantEmail = "ada@example.com",
        decimal principalAmount = 1000m,
        decimal annualInterestRate = 12.5m,
        int termMonths = 24,
        decimal? currentBalance = null)
    {
        return Loan.Create(
            LoanId,
            applicantName,
            applicantEmail,
            principalAmount,
            annualInterestRate,
            termMonths,
            currentBalance,
            CreatedAtUtc);
    }
}
