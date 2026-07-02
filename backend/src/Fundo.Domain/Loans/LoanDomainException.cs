namespace Fundo.Domain.Loans;

public sealed class LoanDomainException : Exception
{
    public LoanDomainException(string message)
        : base(message)
    {
    }
}
