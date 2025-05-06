using CdrApix.Data;
using CdrApix.DTOs;
using CdrApix.Models;
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

        public CdrController(CdrDbContext context)
        {
            _context = context;
        }

        // Uploads and imports CDR records from a CSV file into the database.
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

        // Returns the average call cost from all records.
        [HttpGet("average-cost")]
        public async Task<ActionResult<decimal>> GetAverageCost()
        {
            return await _context.CdrRecords.AverageAsync(r => r.Cost);
        }

        // Retrieves the call record with the highest cost.
        [HttpGet("max-cost")]
        public async Task<IActionResult> GetMaxCostCall()
        {
            var maxCostCall = await _context.CdrRecords
                .OrderByDescending(c => c.Cost)
                .FirstOrDefaultAsync();

            if (maxCostCall == null)
                return NotFound("No records found.");

            return Ok(maxCostCall);
        }

        // Retrieves the call record with the longest duration.
        [HttpGet("longest-call")]
        public async Task<IActionResult> GetLongestCall()
        {
            var longestCall = await _context.CdrRecords
                .OrderByDescending(c => c.Duration)
                .FirstOrDefaultAsync();

            if (longestCall == null)
                return NotFound("No records found.");

            return Ok(longestCall);
        }

        // Calculates the average number of calls made per day.
        [HttpGet("average-calls-per-day")]
        public async Task<IActionResult> GetAverageCallsPerDay()
        {
            var grouped = await _context.CdrRecords
                .GroupBy(c => c.CallDate.Date)
                .Select(g => g.Count())
                .ToListAsync();

            if (!grouped.Any())
                return NotFound("No data to calculate.");

            double average = grouped.Average();
            return Ok(new { averageCallsPerDay = average });
        }

        // Returns total call cost grouped by currency using DTO.
        [HttpGet("total-cost-by-currency")]
        public async Task<ActionResult<IEnumerable<CostByCurrencyDto>>> GetTotalCostByCurrency()
        {
            var totals = await _context.CdrRecords
                .GroupBy(c => c.Currency)
                .Select(g => new CostByCurrencyDto
                {
                    Currency = g.Key,
                    TotalCost = g.Sum(c => c.Cost)
                })
                .ToListAsync();

            return Ok(totals);
        }

        // Lists the top N callers who made the most calls using DTO.
        [HttpGet("top-callers")]
        public async Task<ActionResult<IEnumerable<TopCallerDto>>> GetTopCallers([FromQuery] int n = 5)
        {
            var topCallers = await _context.CdrRecords
                .GroupBy(c => c.CallerId)
                .Select(g => new TopCallerDto
                {
                    CallerId = g.Key,
                    CallCount = g.Count()
                })
                .OrderByDescending(x => x.CallCount)
                .Take(n)
                .ToListAsync();

            return Ok(topCallers);
        }

        // Provides daily summaries: total calls, duration, and cost per date using DTO.
        [HttpGet("daily-summary")]
        public async Task<ActionResult<IEnumerable<CallSummaryDto>>> GetDailySummary()
        {
            var summary = await _context.CdrRecords
                .GroupBy(c => c.CallDate.Date)
                .Select(g => new CallSummaryDto
                {
                    Date = g.Key,
                    TotalCalls = g.Count(),
                    TotalDuration = g.Sum(x => x.Duration),
                    TotalCost = g.Sum(x => x.Cost)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Ok(summary);
        }

        // Returns the number of calls made within a date range.
        [HttpGet("count")]
        public async Task<IActionResult> GetCallCountInRange([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            if (start > end)
                return BadRequest("Start date must be earlier than end date.");

            var count = await _context.CdrRecords
                .CountAsync(c => c.CallDate.Date >= start.Date && c.CallDate.Date <= end.Date);

            return Ok(new { start, end, count });
        }

        // Calculates total call duration for a specific recipient.
        [HttpGet("total-duration")]
        public async Task<IActionResult> GetTotalDurationByRecipient([FromQuery] string recipient)
        {
            if (string.IsNullOrWhiteSpace(recipient))
                return BadRequest("Recipient is required.");

            var totalDuration = await _context.CdrRecords
                .Where(c => c.Recipient == recipient)
                .SumAsync(c => c.Duration);

            return Ok(new { recipient, totalDuration });
        }
    }
}
