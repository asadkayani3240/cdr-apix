using CdrApix.Data;
using CdrApix.DTOs;
using CdrApix.Models;
using Microsoft.EntityFrameworkCore;

namespace CdrApix.Services
{
    public class CdrInsightsService
    {
        private readonly CdrDbContext _context;

        public CdrInsightsService(CdrDbContext context)
        {
            _context = context;
        }
        public async Task<decimal> GetAverageCostAsync()
        {
            return await _context.CdrRecords.AverageAsync(r => r.Cost);
        }
        public async Task<CdrRecord?> GetMaxCostCallAsync()
        {
            return await _context.CdrRecords
                .OrderByDescending(c => c.Cost)
                .FirstOrDefaultAsync();
        }

        public async Task<CdrRecord?> GetLongestCallAsync()
        {
            return await _context.CdrRecords
                .OrderByDescending(c => c.Duration)
                .FirstOrDefaultAsync();
        }

        public async Task<double> GetAverageCallsPerDayAsync()
        {
            var grouped = await _context.CdrRecords
                .GroupBy(c => c.CallDate.Date)
                .Select(g => g.Count())
                .ToListAsync();

            return grouped.Any() ? grouped.Average() : 0;
        }

        public async Task<List<CostByCurrencyDto>> GetTotalCostByCurrencyAsync()
        {
            return await _context.CdrRecords
                .GroupBy(c => c.Currency)
                .Select(g => new CostByCurrencyDto
                {
                    Currency = g.Key,
                    TotalCost = g.Sum(c => c.Cost)
                })
                .ToListAsync();
        }

        public async Task<List<TopCallerDto>> GetTopCallersAsync(int n)
        {
            return await _context.CdrRecords
                .GroupBy(c => c.CallerId)
                .Select(g => new TopCallerDto
                {
                    CallerId = g.Key,
                    CallCount = g.Count()
                })
                .OrderByDescending(x => x.CallCount)
                .Take(n)
                .ToListAsync();
        }

        public async Task<List<CallSummaryDto>> GetDailySummaryAsync()
        {
            return await _context.CdrRecords
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
        }

        public async Task<int> GetCallCountInRangeAsync(DateTime start, DateTime end)
        {
            return await _context.CdrRecords
                .CountAsync(c => c.CallDate.Date >= start.Date && c.CallDate.Date <= end.Date);
        }

        public async Task<int> GetTotalDurationByRecipientAsync(string recipient)
        {
            return await _context.CdrRecords
                .Where(c => c.Recipient == recipient)
                .SumAsync(c => c.Duration);
        }
    }
}
