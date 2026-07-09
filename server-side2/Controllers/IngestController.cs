using Microsoft.AspNetCore.Mvc;
using SQLink.Abstractions;
using SQLink.Contracts;
using SQLink.Services;

namespace SQLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngestController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ITransactionStore _transactionStore;

        public IngestController(ITransactionService transactionService, ITransactionStore transactionStore)
        {
            _transactionService = transactionService;
            _transactionStore = transactionStore;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TransactionRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var acceptedTransaction = await _transactionService.IngestAsync(request);
            return Accepted(acceptedTransaction);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var transactions = _transactionStore.GetAll();
            return Ok(transactions);
        }
    }
}
