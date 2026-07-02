using Fundo.Application.Common;
using Fundo.Application.Loans;
using Fundo.Applications.WebApi.Constants;
using Fundo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using Xunit;

namespace Fundo.Services.Tests.Integration.Fundo.Applications.WebApi.Test.Controllers.Loans
{
    public class LoanManagementControllerTests : IClassFixture<LoanWebApplicationFactory>
    {
        private readonly LoanWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public LoanManagementControllerTests(LoanWebApplicationFactory factory)
        {
            _factory = factory;
            _factory.ResetDatabase();
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task GetList_ShouldReturnLoanSummariesWithoutPaymentHistory()
        {
            // Arrange
            var createdLoan = await CreateLoanAsync("List Contract Test");
            var paymentResponse = await _client.PostAsJsonAsync($"/loans/{createdLoan.Id}/payment", new LoanPaymentRequest
            {
                Amount = 125m,
                PaymentDateUtc = new DateTime(2026, 7, 3, 12, 0, 0, DateTimeKind.Utc),
                Note = "hidden from list"
            });
            var paymentBody = await paymentResponse.Content.ReadAsStringAsync();
            Assert.True(paymentResponse.IsSuccessStatusCode, paymentBody);

            // Act
            var response = await _client.GetAsync("/loans?pageSize=100");
            var body = await response.Content.ReadAsStringAsync();
            var pagedLoans = await response.Content.ReadFromJsonAsync<PagedResult<LoanSummaryDto>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(pagedLoans);
            Assert.Contains(pagedLoans!.Items, loan => loan.Id == createdLoan.Id);
            Assert.Null(typeof(LoanSummaryDto).GetProperty("Payments"));

            using var json = JsonDocument.Parse(body);
            var listedLoan = json.RootElement
                .GetProperty("items")
                .EnumerateArray()
                .Single(loan => loan.GetProperty("id").GetGuid() == createdLoan.Id);

            Assert.False(listedLoan.TryGetProperty("payments", out _));
            Assert.Equal("List Contract Test", listedLoan.GetProperty("applicantName").GetString());
        }

        [Fact]
        public async Task GetList_ShouldReturnDefaultPagination_WhenQueryIsOmitted()
        {
            // Act
            var response = await _client.GetAsync("/loans");
            var pagedLoans = await response.Content.ReadFromJsonAsync<PagedResult<LoanSummaryDto>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(pagedLoans);
            Assert.Equal(1, pagedLoans!.PageNumber);
            Assert.Equal(10, pagedLoans.PageSize);
            Assert.Equal(3, pagedLoans.TotalCount);
            Assert.Equal(1, pagedLoans.TotalPages);
            Assert.False(pagedLoans.HasPreviousPage);
            Assert.False(pagedLoans.HasNextPage);
            Assert.Equal(3, pagedLoans.Items.Count);
            Assert.Equal("Robert Johnson", pagedLoans.Items[0].ApplicantName);
        }

        [Fact]
        public async Task GetList_ShouldReturnRequestedPage_WhenPaginationIsProvided()
        {
            // Act
            var response = await _client.GetAsync("/loans?pageNumber=2&pageSize=2");
            var pagedLoans = await response.Content.ReadFromJsonAsync<PagedResult<LoanSummaryDto>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(pagedLoans);
            Assert.Equal(2, pagedLoans!.PageNumber);
            Assert.Equal(2, pagedLoans.PageSize);
            Assert.Equal(3, pagedLoans.TotalCount);
            Assert.Equal(2, pagedLoans.TotalPages);
            Assert.True(pagedLoans.HasPreviousPage);
            Assert.False(pagedLoans.HasNextPage);
            var loan = Assert.Single(pagedLoans.Items);
            Assert.Equal("John Doe", loan.ApplicantName);
        }

        [Fact]
        public async Task GetList_ShouldReturnEmptyItems_WhenPageIsBeyondAvailableData()
        {
            // Act
            var response = await _client.GetAsync("/loans?pageNumber=3&pageSize=2");
            var pagedLoans = await response.Content.ReadFromJsonAsync<PagedResult<LoanSummaryDto>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(pagedLoans);
            Assert.Equal(3, pagedLoans!.PageNumber);
            Assert.Equal(2, pagedLoans.PageSize);
            Assert.Equal(3, pagedLoans.TotalCount);
            Assert.Equal(2, pagedLoans.TotalPages);
            Assert.True(pagedLoans.HasPreviousPage);
            Assert.False(pagedLoans.HasNextPage);
            Assert.Empty(pagedLoans.Items);
        }

        [Fact]
        public async Task GetList_ShouldReturnBadRequest_WhenPageNumberIsInvalid()
        {
            // Act
            var response = await _client.GetAsync("/loans?pageNumber=0&pageSize=10");
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("pageNumber must be greater than or equal to 1.", body);
        }

        [Fact]
        public async Task GetList_ShouldReturnBadRequest_WhenPageSizeIsInvalid()
        {
            // Act
            var response = await _client.GetAsync("/loans?pageNumber=1&pageSize=0");
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("pageSize must be between 1 and 100.", body);
        }

        [Fact]
        public async Task GetList_ShouldReturnBadRequest_WhenPageSizeExceedsMaximum()
        {
            // Act
            var response = await _client.GetAsync("/loans?pageNumber=1&pageSize=101");
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("pageSize must be between 1 and 100.", body);
        }

        [Fact]
        public async Task GetLoanAlias_ShouldReturnNotFound_WhenRouteIsNotRegistered()
        {
            // Act
            var response = await _client.GetAsync("/loan");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Swagger_ShouldServeUi_WhenEnvironmentIsDevelopment()
        {
            // Arrange
            using var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/swagger");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Cors_ShouldAllowAngularDevelopmentOrigin()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Options, "/loans");
            request.Headers.Add("Origin", "http://localhost:4200");
            request.Headers.Add("Access-Control-Request-Method", "GET");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins));
            Assert.Contains("http://localhost:4200", origins);
        }

        [Fact]
        public async Task UnexpectedException_ShouldReturnGenericErrorWithTraceId()
        {
            // Arrange
            using var client = _factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.RemoveAll<ILoanService>();
                        services.AddScoped<ILoanService, ThrowingLoanService>();
                    });
                })
                .CreateClient();

            // Act
            var response = await client.GetAsync("/loans");
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("application/json", response.Content.Headers.ContentType?.MediaType);
            Assert.DoesNotContain("Simulated infrastructure failure", body);

            using var json = JsonDocument.Parse(body);
            Assert.Equal(ErrorMessages.UnexpectedError, json.RootElement.GetProperty("message").GetString());
            Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("traceId").GetString()));
        }

        [Fact]
        public async Task CreateLoan_ShouldReturnBadRequest_WhenLoanRequestViolatesDomainRules()
        {
            // Arrange
            var request = new CreateLoanRequest
            {
                ApplicantName = "Invalid Create Test",
                ApplicantEmail = $"invalid-create-{Guid.NewGuid():N}@example.com",
                PrincipalAmount = 0m,
                AnnualInterestRate = 5m,
                TermMonths = 12,
                CurrentBalance = 0m
            };

            // Act
            var response = await _client.PostAsJsonAsync("/loans", request);
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Principal amount must be greater than zero.", body);
        }

        [Fact]
        public async Task CreateLoan_ShouldReturnBadRequest_WhenApplicantEmailIsInvalid()
        {
            // Arrange
            var request = new CreateLoanRequest
            {
                ApplicantName = "Invalid Email Test",
                ApplicantEmail = "not-an-email",
                PrincipalAmount = 1000m,
                AnnualInterestRate = 5m,
                TermMonths = 12,
                CurrentBalance = 1000m
            };

            // Act
            var response = await _client.PostAsJsonAsync("/loans", request);
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(nameof(CreateLoanRequest.ApplicantEmail), body);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenLoanDoesNotExist()
        {
            // Arrange
            var loanId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/loans/{loanId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ApplyPayment_ShouldPersistPaymentHistory()
        {
            var createResponse = await _client.PostAsJsonAsync("/loans", new CreateLoanRequest
            {
                ApplicantName = "Payment History Test",
                ApplicantEmail = $"payment-history-{Guid.NewGuid():N}@example.com",
                PrincipalAmount = 1000m,
                AnnualInterestRate = 5m,
                TermMonths = 12,
                CurrentBalance = 1000m
            });
            var createBody = await createResponse.Content.ReadAsStringAsync();
            Assert.True(createResponse.IsSuccessStatusCode, createBody);

            var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanDetailsDto>();
            Assert.NotNull(createdLoan);

            var paymentDateUtc = new DateTime(2026, 7, 3, 12, 0, 0, DateTimeKind.Utc);
            var paymentResponse = await _client.PostAsJsonAsync($"/loans/{createdLoan!.Id}/payment", new LoanPaymentRequest
            {
                Amount = 125m,
                PaymentDateUtc = paymentDateUtc,
                Note = "first API payment"
            });
            var paymentBody = await paymentResponse.Content.ReadAsStringAsync();
            Assert.True(paymentResponse.IsSuccessStatusCode, paymentBody);

            var detailResponse = await _client.GetAsync($"/loans/{createdLoan.Id}");
            var detailBody = await detailResponse.Content.ReadAsStringAsync();
            Assert.True(detailResponse.IsSuccessStatusCode, detailBody);

            using var detailJson = JsonDocument.Parse(detailBody);
            Assert.True(detailJson.RootElement.TryGetProperty("payments", out var payments));
            Assert.Equal(1, payments.GetArrayLength());

            var loanDetails = await detailResponse.Content.ReadFromJsonAsync<LoanDetailsDto>();
            Assert.NotNull(loanDetails);
            Assert.Equal(875m, loanDetails!.CurrentBalance);

            var payment = Assert.Single(loanDetails.Payments);
            Assert.Equal(createdLoan.Id, payment.LoanId);
            Assert.Equal(125m, payment.Amount);
            Assert.Equal(paymentDateUtc, payment.PaymentDateUtc);
            Assert.Equal("first API payment", payment.Note);
        }

        [Fact]
        public async Task ApplyPayment_ShouldReturnNotFound_WhenLoanDoesNotExist()
        {
            // Arrange
            var loanId = Guid.NewGuid();
            var request = new LoanPaymentRequest
            {
                Amount = 10m,
                PaymentDateUtc = new DateTime(2026, 7, 3, 12, 0, 0, DateTimeKind.Utc)
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/loans/{loanId}/payment", request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(-5)]
        [InlineData(1001)]
        public async Task ApplyPayment_ShouldReturnBadRequest_WhenPaymentAmountIsInvalid(decimal paymentAmount)
        {
            // Arrange
            var createResponse = await _client.PostAsJsonAsync("/loans", new CreateLoanRequest
            {
                ApplicantName = "Invalid Payment Test",
                ApplicantEmail = $"invalid-payment-{Guid.NewGuid():N}@example.com",
                PrincipalAmount = 1000m,
                AnnualInterestRate = 5m,
                TermMonths = 12,
                CurrentBalance = 1000m
            });
            var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanDetailsDto>();
            Assert.NotNull(createdLoan);

            // Act
            var paymentResponse = await _client.PostAsJsonAsync($"/loans/{createdLoan!.Id}/payment", new LoanPaymentRequest
            {
                Amount = paymentAmount,
                PaymentDateUtc = new DateTime(2026, 7, 3, 12, 0, 0, DateTimeKind.Utc)
            });

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, paymentResponse.StatusCode);
        }

        private async Task<LoanDetailsDto> CreateLoanAsync(string applicantName)
        {
            var createResponse = await _client.PostAsJsonAsync("/loans", new CreateLoanRequest
            {
                ApplicantName = applicantName,
                ApplicantEmail = $"{applicantName.Replace(" ", "-", StringComparison.OrdinalIgnoreCase).ToLowerInvariant()}-{Guid.NewGuid():N}@example.com",
                PrincipalAmount = 1000m,
                AnnualInterestRate = 5m,
                TermMonths = 12,
                CurrentBalance = 1000m
            });
            var createBody = await createResponse.Content.ReadAsStringAsync();
            Assert.True(createResponse.IsSuccessStatusCode, createBody);

            var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanDetailsDto>();
            Assert.NotNull(createdLoan);
            return createdLoan!;
        }
    }

    public sealed class LoanWebApplicationFactory : WebApplicationFactory<global::Fundo.Applications.WebApi.Startup>
    {
        public void ResetDatabase()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<LoanDbContext>();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            var databaseName = $"FundoLoansTests-{Guid.NewGuid():N}";

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<LoanDbContext>>();
                services.AddDbContext<LoanDbContext>(options =>
                    options.UseInMemoryDatabase(databaseName));

                using var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<LoanDbContext>();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
            });
        }
    }

    internal sealed class ThrowingLoanService : ILoanService
    {
        public Task<Result<LoanDetailsDto>> CreateAsync(CreateLoanRequest request, CancellationToken cancellationToken = default)
        {
            throw CreateException();
        }

        public Task<Result<IPagedResult<LoanSummaryDto>>> GetListAsync(PaginationRequest pagination, CancellationToken cancellationToken = default)
        {
            throw CreateException();
        }

        public Task<Result<LoanDetailsDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw CreateException();
        }

        public Task<Result<LoanDetailsDto>> ApplyPaymentAsync(Guid id, LoanPaymentRequest request, CancellationToken cancellationToken = default)
        {
            throw CreateException();
        }

        private static InvalidOperationException CreateException()
        {
            return new InvalidOperationException("Simulated infrastructure failure.");
        }
    }
}
