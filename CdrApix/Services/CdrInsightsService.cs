using CdrApix.Data; 
using CdrApix.DTOs;
using CdrApix.Models;
using Microsoft.EntityFrameworkCore;

namespace CdrApix.Services
{
    public class CdrInsightsService : ICdrInsightsService
    {
        private readonly CdrDbContext _db;
        public CdrInsightsService(CdrDbContext db) => _db = db;

        public Task<decimal> GetAverageCostAsync() =>
            _db.CdrRecords.AverageAsync(r => r.Cost);

        public Task<CdrRecord?> GetMaxCostCallAsync() =>
            _db.CdrRecords.OrderByDescending(r => r.Cost).FirstOrDefaultAsync();

        public Task<CdrRecord?> GetLongestCallAsync() =>
            _db.CdrRecords.OrderByDescending(r => r.Duration).FirstOrDefaultAsync();

        public async Task<double> GetAverageCallsPerDayAsync()
        {
            var counts = await _db.CdrRecords
                .GroupBy(r => r.CallDate.Date)
                .Select(g => g.Count())
                .ToListAsync();
            return counts.Any() ? counts.Average() : 0;
        }

        public Task<List<CostByCurrencyDto>> GetTotalCostByCurrencyAsync() =>
            _db.CdrRecords
               .GroupBy(r => r.Currency)
               .Select(g => new CostByCurrencyDto
               {
                   Currency = g.Key,
                   TotalCost = g.Sum(r => r.Cost)
               })
               .ToListAsync();

        public Task<List<TopCallerDto>> GetTopCallersAsync(int n) =>
            _db.CdrRecords
               .Where(r => !string.IsNullOrEmpty(r.CallerId))
               .GroupBy(r => r.CallerId)
               .Select(g => new TopCallerDto
               {
                   CallerId = g.Key,
                   CallCount = g.Count()
               })
               .OrderByDescending(x => x.CallCount)
               .Take(n)
               .ToListAsync();

        public Task<List<CallSummaryDto>> GetDailySummaryAsync() =>
            _db.CdrRecords
               .GroupBy(r => r.CallDate.Date)
               .Select(g => new CallSummaryDto
               {
                   Date = g.Key,
                   TotalCalls = g.Count(),
                   TotalDuration = g.Sum(r => r.Duration),
                   TotalCost = g.Sum(r => r.Cost)
               })
               .OrderBy(x => x.Date)
               .ToListAsync();

        public Task<int> GetCallCountInRangeAsync(DateTime start, DateTime end) =>
            _db.CdrRecords.CountAsync(r =>
                r.CallDate.Date >= start.Date &&
                r.CallDate.Date <= end.Date);

        public Task<int> GetTotalDurationByRecipientAsync(string recipient) =>
            _db.CdrRecords
               .Where(r => r.Recipient == recipient)
               .SumAsync(r => r.Duration);
    }
}
