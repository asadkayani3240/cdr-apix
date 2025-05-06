using CdrApix.DTOs;
using CdrApix.Models;

namespace CdrApix.Services
{
    public interface ICdrInsightsService
    {
        Task<List<CostByCurrencyDto>> GetTotalCostByCurrencyAsync();
        Task<List<TopCallerDto>> GetTopCallersAsync(int n); 
        Task<List<CallSummaryDto>> GetDailySummaryAsync(); 
        Task<decimal> GetAverageCostAsync();
        Task<CdrRecord?> GetMaxCostCallAsync();
        Task<CdrRecord?> GetLongestCallAsync();
        Task<double> GetAverageCallsPerDayAsync();
        Task<int> GetCallCountInRangeAsync(DateTime start, DateTime end);
        Task<int> GetTotalDurationByRecipientAsync(string recipient);
    }
}
