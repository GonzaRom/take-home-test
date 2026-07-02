using Fundo.Domain.Loans;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Infrastructure.Persistence;

public sealed class LoanDbContext : DbContext
{
    public LoanDbContext(DbContextOptions<LoanDbContext> options)
        : base(options)
    {
    }

    public DbSet<Loan> Loans => Set<Loan>();

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.ToTable("Loans");
            entity.HasKey(loan => loan.Id);

            entity.Property(loan => loan.PrincipalAmount).HasPrecision(18, 2);
            entity.Property(loan => loan.AnnualInterestRate).HasPrecision(9, 4);
            entity.Property(loan => loan.TermMonths).IsRequired();
            entity.Property(loan => loan.CurrentBalance).HasPrecision(18, 2);
            entity.Property(loan => loan.ApplicantName).HasMaxLength(200).IsRequired();
            entity.Property(loan => loan.ApplicantEmail).HasMaxLength(320).IsRequired();
            entity.Property(loan => loan.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(loan => loan.CreatedAtUtc).IsRequired();

            entity.HasMany(loan => loan.Payments)
                .WithOne()
                .HasForeignKey(payment => payment.LoanId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(loan => loan.Payments)
                .HasField("payments")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.HasData(
                new
                {
                    Id = Guid.Parse("d9d11d7c-96c0-4028-a695-5b97a1236101"),
                    PrincipalAmount = 25000m,
                    AnnualInterestRate = 8.5m,
                    TermMonths = 60,
                    CurrentBalance = 25000m,
                    ApplicantName = "John Doe",
                    ApplicantEmail = "john.doe@example.com",
                    Status = LoanStatus.Active,
                    CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new
                {
                    Id = Guid.Parse("b7809481-ac07-472b-84d9-71726ab24470"),
                    PrincipalAmount = 15000m,
                    AnnualInterestRate = 6.75m,
                    TermMonths = 36,
                    CurrentBalance = 15000m,
                    ApplicantName = "Jane Smith",
                    ApplicantEmail = "jane.smith@example.com",
                    Status = LoanStatus.Active,
                    CreatedAtUtc = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)
                },
                new
                {
                    Id = Guid.Parse("9a8cd754-c81a-4bb9-98d9-b4a4e86155ef"),
                    PrincipalAmount = 50000m,
                    AnnualInterestRate = 10.25m,
                    TermMonths = 84,
                    CurrentBalance = 50000m,
                    ApplicantName = "Robert Johnson",
                    ApplicantEmail = "robert.johnson@example.com",
                    Status = LoanStatus.Active,
                    CreatedAtUtc = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc)
                });
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(payment => payment.Id);

            entity.Property(payment => payment.Id).ValueGeneratedNever();
            entity.Property(payment => payment.Amount).HasPrecision(18, 2);
            entity.Property(payment => payment.PaymentDateUtc).IsRequired();
            entity.Property(payment => payment.Note).HasMaxLength(500);
        });
    }
}
