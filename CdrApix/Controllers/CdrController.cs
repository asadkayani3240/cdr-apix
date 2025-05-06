using CdrApix.Data;
using CdrApix.DTOs;
using CdrApix.Models;
using CdrApix.Services;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CdrApix.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CdrController : ControllerBase
    {
        private readonly CdrDbContext _context;
        private readonly CdrInsightsService _insightsService;

        public CdrController(CdrDbContext context, CdrInsightsService insightsService)
        {
            _context = context;
            _insightsService = insightsService;
        }

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

        [HttpGet("average-cost")]
        public async Task<ActionResult<decimal>> GetAverageCost()
        {
            return await _insightsService.GetAverageCostAsync();
        }

        [HttpGet("max-cost")]
        public async Task<ActionResult<CdrRecord>> GetMaxCostCall()
        {
            var result = await _insightsService.GetMaxCostCallAsync();
            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet("longest-call")]
        public async Task<ActionResult<CdrRecord>> GetLongestCall()
        {
            var result = await _insightsService.GetLongestCallAsync();
            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet("average-calls-per-day")]
        public async Task<ActionResult<object>> GetAverageCallsPerDay()
        {
            var result = await _insightsService.GetAverageCallsPerDayAsync();
            return Ok(new { averageCallsPerDay = result });
        }

        [HttpGet("total-cost-by-currency")]
        public async Task<ActionResult<IEnumerable<CostByCurrencyDto>>> GetTotalCostByCurrency()
        {
            return Ok(await _insightsService.GetTotalCostByCurrencyAsync());
        }

        [HttpGet("top-callers")]
        public async Task<ActionResult<IEnumerable<TopCallerDto>>> GetTopCallers([FromQuery] int n = 5)
        {
            return Ok(await _insightsService.GetTopCallersAsync(n));
        }

        [HttpGet("daily-summary")]
        public async Task<ActionResult<IEnumerable<CallSummaryDto>>> GetDailySummary()
        {
            return Ok(await _insightsService.GetDailySummaryAsync());
        }

        [HttpGet("count")]
        public async Task<ActionResult<object>> GetCallCountInRange([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            if (start > end)
                return BadRequest("Start date must be earlier than end date.");

            var count = await _insightsService.GetCallCountInRangeAsync(start, end);
            return Ok(new { start, end, count });
        }

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
