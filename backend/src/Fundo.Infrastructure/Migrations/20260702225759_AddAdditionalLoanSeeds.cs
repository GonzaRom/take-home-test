using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fundo.Infrastructure.Migrations
{
    public partial class AddAdditionalLoanSeeds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Loans",
                columns: new[] { "Id", "AnnualInterestRate", "ApplicantEmail", "ApplicantName", "CreatedAtUtc", "CurrentBalance", "PrincipalAmount", "Status", "TermMonths" },
                values: new object[,]
                {
                    { new Guid("1df28770-bef2-473b-bcb2-dc1de930f3c2"), 5.95m, "priya.patel@example.com", "Priya Patel", new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc), 12000m, 12000m, "Active", 24 },
                    { new Guid("43e372c8-1339-420d-9a50-a2d08bec7d99"), 10.75m, "nadia.flores@example.com", "Nadia Flores", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 47000m, 47000m, "Active", 84 },
                    { new Guid("4d5c57eb-f233-4fa3-8ab9-e57a7631dc99"), 7.75m, "marcus.brown@example.com", "Marcus Brown", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), 22000m, 22000m, "Active", 48 },
                    { new Guid("5d5998e9-1e22-4b7d-858b-3cbf9dc7da9d"), 8.15m, "grace.wilson@example.com", "Grace Wilson", new DateTime(2026, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc), 28500m, 28500m, "Active", 60 },
                    { new Guid("5ee054d0-8216-4fa8-8d65-f4418cf49c8e"), 4.8m, "sofia.rivera@example.com", "Sofia Rivera", new DateTime(2026, 1, 9, 0, 0, 0, 0, DateTimeKind.Utc), 9500m, 9500m, "Active", 18 },
                    { new Guid("63992150-4aa2-4eeb-a3d1-778ebb0150c3"), 7.05m, "ethan.brooks@example.com", "Ethan Brooks", new DateTime(2026, 1, 14, 0, 0, 0, 0, DateTimeKind.Utc), 19500m, 19500m, "Active", 48 },
                    { new Guid("9edbaab6-a4dd-4640-bb74-9979b6ff0d8a"), 9.1m, "michael.chen@example.com", "Michael Chen", new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), 32000m, 32000m, "Active", 72 },
                    { new Guid("a0034e8a-58c9-4431-a53b-2a1f05be9a19"), 9.85m, "hannah.lee@example.com", "Hannah Lee", new DateTime(2026, 1, 11, 0, 0, 0, 0, DateTimeKind.Utc), 36000m, 36000m, "Active", 72 },
                    { new Guid("b834149b-e9be-4c74-a7b0-82e066db3618"), 6.35m, "omar.hassan@example.com", "Omar Hassan", new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), 14500m, 14500m, "Active", 36 },
                    { new Guid("e3631e12-8a99-4741-988b-80d8548b85d6"), 7.2m, "alicia.martinez@example.com", "Alicia Martinez", new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), 18000m, 18000m, "Active", 48 },
                    { new Guid("f7c15d77-10ce-40b6-bd97-66c0a1025f3c"), 11.5m, "daniel.kim@example.com", "Daniel Kim", new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Utc), 41000m, 41000m, "Active", 84 },
                    { new Guid("f897e9d6-c0b6-4667-bc04-359d506805da"), 8.95m, "elena.garcia@example.com", "Elena Garcia", new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Utc), 27500m, 27500m, "Active", 60 }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Loans",
                keyColumn: "Id",
                keyValue: new Guid("1df28770-bef2-473b-bcb2-dc1de930f3c2"));

            migrationBuilder.DeleteData(
                table: "Loans",
                keyColumn: "Id",
                keyValue: new Guid("43e372c8-1339-420d-9a50-a2d08bec7d99"));

            migrationBuilder.DeleteData(
                table: "Loans",
                keyColumn: "Id",
                keyValue: new Guid("4d5c57eb-f233-4fa3-8ab9-e57a7631dc99"));

            migrationBuilder.DeleteData(
                table: "Loans",
                keyColumn: "Id",
                keyValue: new Guid("5d5998e9-1e22-4b7d-858b-3cbf9dc7da9d"));

            migrationBuilder.DeleteData(
                table: "Loans",
                keyColumn: "Id",
                keyValue: new Guid("5ee054d0-8216-4fa8-8d65-f4418cf49c8e"));

            migrationBuilder.DeleteData(
                table: "Loans",
                keyColumn: "Id",
                keyValue: new Guid("63992150-4aa2-4eeb-a3d1-778ebb0150c3"));

            migrationBuilder.DeleteData(
                table: "Loans",
                keyColumn: "Id",
                keyValue: new Guid("9edbaab6-a4dd-4640-bb74-9979b6ff0d8a"));

            migrationBuilder.DeleteData(
                table: "Loans",
                keyColumn: "Id",
                keyValue: new Guid("a0034e8a-58c9-4431-a53b-2a1f05be9a19"));

            migrationBuilder.DeleteData(
                table: "Loans",
                keyColumn: "Id",
                keyValue: new Guid("b834149b-e9be-4c74-a7b0-82e066db3618"));

            migrationBuilder.DeleteData(
                table: "Loans",
                keyColumn: "Id",
                keyValue: new Guid("e3631e12-8a99-4741-988b-80d8548b85d6"));

            migrationBuilder.DeleteData(
                table: "Loans",
                keyColumn: "Id",
                keyValue: new Guid("f7c15d77-10ce-40b6-bd97-66c0a1025f3c"));

            migrationBuilder.DeleteData(
                table: "Loans",
                keyColumn: "Id",
                keyValue: new Guid("f897e9d6-c0b6-4667-bc04-359d506805da"));
        }
    }
}
