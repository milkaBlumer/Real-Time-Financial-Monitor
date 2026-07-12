using Microsoft.AspNetCore.Mvc;
using SQLink.Abstractions;
using SQLink.Models;
using SQLink.Services;

namespace SQLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngestController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<IngestController> _logger;

        public IngestController(
            ITransactionService transactionService,
            ILogger<IngestController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Transaction request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var acceptedTransaction = await _transactionService.IngestAsync(request);
                return Accepted(acceptedTransaction);
            }
            catch (TransactionConflictException ex)
            {
                return Conflict(new
                {
                    message = ex.Message,
                    id = ex.Id
                });
            }
            catch (RealtimePublishException ex)
            {
                _logger.LogWarning(ex, "Realtime publish failed for transaction {TransactionId}", ex.Id);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    message = "Transaction persisted, but realtime delivery is temporarily unavailable.",
                    id = ex.Id
                });
            }
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var transactions = _transactionService.GetAll();
            return Ok(transactions);
        }
    }
}
