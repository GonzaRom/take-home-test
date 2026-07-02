using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fundo.Application.Common;
using Fundo.Application.Loans;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fundo.Applications.WebApi.Controllers
{
    [ApiController]
    [Route("loans")]
    public sealed class LoanManagementController : ControllerBase
    {
        private readonly ILoanService loanService;

        public LoanManagementController(ILoanService loanService)
        {
            this.loanService = loanService;
        }

        /// <summary>
        /// Creates a loan application and returns the persisted loan details.
        /// </summary>
        /// <param name="request">Loan application details, including applicant data, principal, interest rate, term and starting balance.</param>
        /// <param name="cancellationToken">Request cancellation token.</param>
        /// <returns>The created loan.</returns>
        [ProducesResponseType(typeof(LoanDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult<LoanDetailsDto>> CreateAsync(CreateLoanRequest request, CancellationToken cancellationToken)
        {
            var result = await loanService.CreateAsync(request, cancellationToken);

            return ToActionResult(result, loan => Created($"/loans/{loan.Id}", loan));
        }

        /// <summary>
        /// Lists the available loan summaries for the loan management view.
        /// </summary>
        /// <param name="cancellationToken">Request cancellation token.</param>
        /// <returns>Loan summaries ordered for display.</returns>
        [ProducesResponseType(typeof(IReadOnlyList<LoanSummaryDto>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<LoanSummaryDto>>> GetListAsync(CancellationToken cancellationToken)
        {
            var result = await loanService.GetListAsync(cancellationToken);

            return ToActionResult(result, loans => Ok(loans));
        }

        /// <summary>
        /// Retrieves a single loan with its payment history.
        /// </summary>
        /// <param name="id">Loan identifier.</param>
        /// <param name="cancellationToken">Request cancellation token.</param>
        /// <returns>The matching loan when it exists.</returns>
        [ProducesResponseType(typeof(LoanDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<LoanDetailsDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var result = await loanService.GetByIdAsync(id, cancellationToken);

            return ToActionResult(result, loan => Ok(loan));
        }

        /// <summary>
        /// Registers a payment against an active loan and returns the updated loan balance.
        /// </summary>
        /// <param name="id">Loan identifier.</param>
        /// <param name="request">Payment amount, optional UTC payment date and optional note.</param>
        /// <param name="cancellationToken">Request cancellation token.</param>
        /// <returns>The updated loan after applying the payment.</returns>
        [ProducesResponseType(typeof(LoanDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("{id:guid}/payment")]
        public async Task<ActionResult<LoanDetailsDto>> ApplyPaymentAsync(Guid id, LoanPaymentRequest request, CancellationToken cancellationToken)
        {
            var result = await loanService.ApplyPaymentAsync(id, request, cancellationToken);

            return ToActionResult(result, loan => Ok(loan));
        }

        private ActionResult<T> ToActionResult<T>(Result<T> result, Func<T, ActionResult<T>> successResult)
        {
            switch (result.Status)
            {
                case ResultStatus.Success:
                    return successResult(result.Value);
                case ResultStatus.Invalid:
                    return BadRequest(result.Error);
                case ResultStatus.NotFound:
                    return NotFound();
                default:
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
