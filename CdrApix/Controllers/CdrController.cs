using CdrApix.Data;
using CdrApix.DTOs;
using CdrApix.Models;
using CdrApix.Services;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CdrApix.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CdrController : ControllerBase
    {
        private readonly CdrDbContext _context;
        private readonly ICdrInsightsService _insightsService;

        public CdrController(
            CdrDbContext context,
            ICdrInsightsService insightsService)      // <-- interface here
        {
            _context = context;
            _insightsService = insightsService;
        }

        /// <summary>
        /// Uploads and imports CDR records from a CSV file into the database.
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Context.RegisterClassMap<CdrRecordMap>();

            var records = csv.GetRecords<CdrRecord>().ToList();

            _context.CdrRecords.AddRange(records);
            await _context.SaveChangesAsync();

            return Ok($"{records.Count} records uploaded successfully.");
        }

        /// <summary>
        /// Returns the average call cost across all records.
        /// </summary>
        [HttpGet("average-cost")]
        public async Task<ActionResult<decimal>> GetAverageCost()
            => Ok(await _insightsService.GetAverageCostAsync());

        /// <summary>
        /// Retrieves the call record with the highest cost.
        /// </summary>
        [HttpGet("max-cost")]
        public async Task<ActionResult<CdrRecord>> GetMaxCostCall()
        {
            var dto = await _insightsService.GetMaxCostCallAsync();
            return dto != null ? Ok(dto) : NotFound();
        }

        /// <summary>
        /// Retrieves the call record with the longest duration.
        /// </summary>
        [HttpGet("longest-call")]
        public async Task<ActionResult<CdrRecord>> GetLongestCall()
        {
            var dto = await _insightsService.GetLongestCallAsync();
            return dto != null ? Ok(dto) : NotFound();
        }

        /// <summary>
        /// Calculates the average number of calls made per day.
        /// </summary>
        [HttpGet("average-calls-per-day")]
        public async Task<ActionResult<object>> GetAverageCallsPerDay()
        {
            var avg = await _insightsService.GetAverageCallsPerDayAsync();
            return Ok(new { averageCallsPerDay = avg });
        }

        /// <summary>
        /// Returns total call cost grouped by currency.
        /// </summary>
        [HttpGet("total-cost-by-currency")]
        public async Task<ActionResult<IEnumerable<CostByCurrencyDto>>> GetTotalCostByCurrency()
            => Ok(await _insightsService.GetTotalCostByCurrencyAsync());

        /// <summary>
        /// Lists the top N callers by number of calls.
        /// </summary>
        [HttpGet("top-callers")]
        public async Task<ActionResult<IEnumerable<TopCallerDto>>> GetTopCallers([FromQuery] int n = 5)
            => Ok(await _insightsService.GetTopCallersAsync(n));

        /// <summary>
        /// Provides daily summaries: total calls, duration, and cost per date.
        /// </summary>
        [HttpGet("daily-summary")]
        public async Task<ActionResult<IEnumerable<CallSummaryDto>>> GetDailySummary()
            => Ok(await _insightsService.GetDailySummaryAsync());

        /// <summary>
        /// Returns the number of calls made within a date range.
        /// </summary>
        [HttpGet("count")]
        public async Task<ActionResult<object>> GetCallCountInRange(
            [FromQuery] DateTime start,
            [FromQuery] DateTime end)
        {
            if (start > end)
                return BadRequest("Start date must be earlier than end date.");

            var count = await _insightsService.GetCallCountInRangeAsync(start, end);
            return Ok(new { start, end, count });
        }

        /// <summary>
        /// Calculates total call duration for a specific recipient.
        /// </summary>
        [HttpGet("total-duration")]
        public async Task<ActionResult<object>> GetTotalDurationByRecipient([FromQuery] string recipient)
        {
            if (string.IsNullOrWhiteSpace(recipient))
                return BadRequest("Recipient is required.");

            var totalDuration = await _insightsService.GetTotalDurationByRecipientAsync(recipient);
            return Ok(new { recipient, totalDuration });
        }
    }
}
