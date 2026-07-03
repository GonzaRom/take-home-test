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
                CreateLoanSeed("d9d11d7c-96c0-4028-a695-5b97a1236101", 25000m, 8.5m, 60, "John Doe", "john.doe@example.com", 1),
                CreateLoanSeed("b7809481-ac07-472b-84d9-71726ab24470", 15000m, 6.75m, 36, "Jane Smith", "jane.smith@example.com", 2),
                CreateLoanSeed("9a8cd754-c81a-4bb9-98d9-b4a4e86155ef", 50000m, 10.25m, 84, "Robert Johnson", "robert.johnson@example.com", 3),
                CreateLoanSeed("e3631e12-8a99-4741-988b-80d8548b85d6", 18000m, 7.2m, 48, "Alicia Martinez", "alicia.martinez@example.com", 4),
                CreateLoanSeed("9edbaab6-a4dd-4640-bb74-9979b6ff0d8a", 32000m, 9.1m, 72, "Michael Chen", "michael.chen@example.com", 5),
                CreateLoanSeed("1df28770-bef2-473b-bcb2-dc1de930f3c2", 12000m, 5.95m, 24, "Priya Patel", "priya.patel@example.com", 6),
                CreateLoanSeed("f897e9d6-c0b6-4667-bc04-359d506805da", 27500m, 8.95m, 60, "Elena Garcia", "elena.garcia@example.com", 7),
                CreateLoanSeed("f7c15d77-10ce-40b6-bd97-66c0a1025f3c", 41000m, 11.5m, 84, "Daniel Kim", "daniel.kim@example.com", 8),
                CreateLoanSeed("5ee054d0-8216-4fa8-8d65-f4418cf49c8e", 9500m, 4.8m, 18, "Sofia Rivera", "sofia.rivera@example.com", 9),
                CreateLoanSeed("4d5c57eb-f233-4fa3-8ab9-e57a7631dc99", 22000m, 7.75m, 48, "Marcus Brown", "marcus.brown@example.com", 10),
                CreateLoanSeed("a0034e8a-58c9-4431-a53b-2a1f05be9a19", 36000m, 9.85m, 72, "Hannah Lee", "hannah.lee@example.com", 11),
                CreateLoanSeed("b834149b-e9be-4c74-a7b0-82e066db3618", 14500m, 6.35m, 36, "Omar Hassan", "omar.hassan@example.com", 12),
                CreateLoanSeed("5d5998e9-1e22-4b7d-858b-3cbf9dc7da9d", 28500m, 8.15m, 60, "Grace Wilson", "grace.wilson@example.com", 13),
                CreateLoanSeed("63992150-4aa2-4eeb-a3d1-778ebb0150c3", 19500m, 7.05m, 48, "Ethan Brooks", "ethan.brooks@example.com", 14),
                CreateLoanSeed("43e372c8-1339-420d-9a50-a2d08bec7d99", 47000m, 10.75m, 84, "Nadia Flores", "nadia.flores@example.com", 15));
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

    private static object CreateLoanSeed(
        string id,
        decimal principalAmount,
        decimal annualInterestRate,
        int termMonths,
        string applicantName,
        string applicantEmail,
        int createdDay)
    {
        return new
        {
            Id = Guid.Parse(id),
            PrincipalAmount = principalAmount,
            AnnualInterestRate = annualInterestRate,
            TermMonths = termMonths,
            CurrentBalance = principalAmount,
            ApplicantName = applicantName,
            ApplicantEmail = applicantEmail,
            Status = LoanStatus.Active,
            CreatedAtUtc = new DateTime(2026, 1, createdDay, 0, 0, 0, DateTimeKind.Utc)
        };
    }
}
