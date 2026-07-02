using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fundo.Infrastructure.Migrations
{
    public partial class InitialLoanSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Loans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicantName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApplicantEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    PrincipalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AnnualInterestRate = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    TermMonths = table.Column<int>(type: "int", nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Loans",
                columns: new[] { "Id", "AnnualInterestRate", "ApplicantEmail", "ApplicantName", "CreatedAtUtc", "CurrentBalance", "PrincipalAmount", "Status", "TermMonths" },
                values: new object[] { new Guid("9a8cd754-c81a-4bb9-98d9-b4a4e86155ef"), 10.25m, "robert.johnson@example.com", "Robert Johnson", new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), 50000m, 50000m, "Active", 84 });

            migrationBuilder.InsertData(
                table: "Loans",
                columns: new[] { "Id", "AnnualInterestRate", "ApplicantEmail", "ApplicantName", "CreatedAtUtc", "CurrentBalance", "PrincipalAmount", "Status", "TermMonths" },
                values: new object[] { new Guid("b7809481-ac07-472b-84d9-71726ab24470"), 6.75m, "jane.smith@example.com", "Jane Smith", new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), 15000m, 15000m, "Active", 36 });

            migrationBuilder.InsertData(
                table: "Loans",
                columns: new[] { "Id", "AnnualInterestRate", "ApplicantEmail", "ApplicantName", "CreatedAtUtc", "CurrentBalance", "PrincipalAmount", "Status", "TermMonths" },
                values: new object[] { new Guid("d9d11d7c-96c0-4028-a695-5b97a1236101"), 8.5m, "john.doe@example.com", "John Doe", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 25000m, 25000m, "Active", 60 });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_LoanId",
                table: "Payments",
                column: "LoanId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Loans");
        }
    }
}
