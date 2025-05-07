using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CdrApix.Data;
using CdrApix.Models;
using CdrApix.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CdrApix.Tests
{
    public class CdrInsightsServiceTests
    {
        private CdrDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<CdrDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new CdrDbContext(options);
        }

        [Fact]
        public async Task GetAverageCostAsync_ReturnsCorrectAverage()
        {
            using var ctx = CreateInMemoryContext();
            ctx.CdrRecords.AddRange(
                new CdrRecord { Cost = 1m, CallerId = "A", Recipient = "X", CallDate = DateTime.Today, Duration = 10, Reference = "R1", Currency = "GBP" },
                new CdrRecord { Cost = 3m, CallerId = "A", Recipient = "X", CallDate = DateTime.Today, Duration = 20, Reference = "R2", Currency = "GBP" },
                new CdrRecord { Cost = 5m, CallerId = "A", Recipient = "X", CallDate = DateTime.Today, Duration = 30, Reference = "R3", Currency = "GBP" }
            );
            await ctx.SaveChangesAsync();

            var svc = new CdrInsightsService(ctx);
            var avg = await svc.GetAverageCostAsync();

            Assert.Equal(3.0m, avg);
        }

        [Fact]
        public async Task GetMaxCostCallAsync_ReturnsRecordWithMaxCost()
        {
            using var ctx = CreateInMemoryContext();
            ctx.CdrRecords.AddRange(
                new CdrRecord { Cost = 2m, Reference = "R1", CallerId = "A", Recipient = "X", CallDate = DateTime.Today, Duration = 1, Currency = "GBP" },
                new CdrRecord { Cost = 7m, Reference = "R2", CallerId = "B", Recipient = "Y", CallDate = DateTime.Today, Duration = 2, Currency = "GBP" },
                new CdrRecord { Cost = 5m, Reference = "R3", CallerId = "C", Recipient = "Z", CallDate = DateTime.Today, Duration = 3, Currency = "GBP" }
            );
            await ctx.SaveChangesAsync();

            var svc = new CdrInsightsService(ctx);
            var max = await svc.GetMaxCostCallAsync();

            Assert.NotNull(max);
            Assert.Equal("R2", max.Reference);
        }

        [Fact]
        public async Task GetLongestCallAsync_ReturnsRecordWithMaxDuration()
        {
            using var ctx = CreateInMemoryContext();
            ctx.CdrRecords.AddRange(
                new CdrRecord { Duration = 10, Reference = "R1", Cost = 1m, CallerId = "A", Recipient = "X", CallDate = DateTime.Today, Currency = "GBP" },
                new CdrRecord { Duration = 30, Reference = "R2", Cost = 2m, CallerId = "B", Recipient = "Y", CallDate = DateTime.Today, Currency = "GBP" },
                new CdrRecord { Duration = 20, Reference = "R3", Cost = 3m, CallerId = "C", Recipient = "Z", CallDate = DateTime.Today, Currency = "GBP" }
            );
            await ctx.SaveChangesAsync();

            var svc = new CdrInsightsService(ctx);
            var longest = await svc.GetLongestCallAsync();

            Assert.NotNull(longest);
            Assert.Equal("R2", longest.Reference);
        }

        [Fact]
        public async Task GetAverageCallsPerDayAsync_ReturnsCorrectAverage()
        {
            using var ctx = CreateInMemoryContext();
            // Day 1: 2 calls, Day 2: 4 calls => average = 3.0
            var d1 = DateTime.Today;
            var d2 = d1.AddDays(1);
            ctx.CdrRecords.AddRange(
                Enumerable.Range(1, 2).Select(i => new CdrRecord { CallDate = d1, Reference = "R" + i, CallerId = "A", Recipient = "X", Duration = 1, Cost = 1m, Currency = "GBP" })
            );
            ctx.CdrRecords.AddRange(
                Enumerable.Range(3, 4).Select(i => new CdrRecord { CallDate = d2, Reference = "R" + i, CallerId = "B", Recipient = "Y", Duration = 1, Cost = 1m, Currency = "GBP" })
            );
            await ctx.SaveChangesAsync();

            var svc = new CdrInsightsService(ctx);
            var avg = await svc.GetAverageCallsPerDayAsync();

            Assert.Equal(3.0, avg, 3);
        }

        [Fact]
        public async Task GetTotalCostByCurrencyAsync_ReturnsGroupedSums()
        {
            using var ctx = CreateInMemoryContext();
            ctx.CdrRecords.AddRange(
                new CdrRecord { Currency = "GBP", Cost = 1m, Reference = "R1", CallerId = "A", Recipient = "X", CallDate = DateTime.Today, Duration = 1 },
                new CdrRecord { Currency = "USD", Cost = 2m, Reference = "R2", CallerId = "B", Recipient = "Y", CallDate = DateTime.Today, Duration = 1 },
                new CdrRecord { Currency = "GBP", Cost = 3m, Reference = "R3", CallerId = "C", Recipient = "Z", CallDate = DateTime.Today, Duration = 1 }
            );
            await ctx.SaveChangesAsync();

            var svc = new CdrInsightsService(ctx);
            var totals = (await svc.GetTotalCostByCurrencyAsync()).ToDictionary(x => x.Currency, x => x.TotalCost);

            Assert.Equal(4m, totals["GBP"]);
            Assert.Equal(2m, totals["USD"]);
        }

        [Fact]
        public async Task GetTopCallersAsync_ReturnsTopNCallers()
        {
            using var ctx = CreateInMemoryContext();
            // A:3 calls, B:1, C:2
            ctx.CdrRecords.AddRange(
                Enumerable.Range(1, 3).Select(i => new CdrRecord { CallerId = "A", Reference = "R" + i, Recipient = "X", CallDate = DateTime.Today, Duration = 1, Cost = 1m, Currency = "GBP" })
            );
            ctx.CdrRecords.AddRange(
                Enumerable.Range(4, 1).Select(i => new CdrRecord { CallerId = "B", Reference = "R" + i, Recipient = "Y", CallDate = DateTime.Today, Duration = 1, Cost = 1m, Currency = "GBP" })
            );
            ctx.CdrRecords.AddRange(
                Enumerable.Range(5, 2).Select(i => new CdrRecord { CallerId = "C", Reference = "R" + i, Recipient = "Z", CallDate = DateTime.Today, Duration = 1, Cost = 1m, Currency = "GBP" })
            );
            await ctx.SaveChangesAsync();

            var svc = new CdrInsightsService(ctx);
            var top2 = (await svc.GetTopCallersAsync(2)).ToList();

            Assert.Equal("A", top2[0].CallerId);
            Assert.Equal(3, top2[0].CallCount);
            Assert.Equal("C", top2[1].CallerId);
            Assert.Equal(2, top2[1].CallCount);
        }

        [Fact]
        public async Task GetDailySummaryAsync_ReturnsCorrectSummaries()
        {
            using var ctx = CreateInMemoryContext();
            var d1 = DateTime.Today;
            var d2 = d1.AddDays(1);

            // Day1: two calls: durations 10+20, costs 1+2
            ctx.CdrRecords.Add(new CdrRecord { CallDate = d1, Duration = 10, Cost = 1m, Reference = "R1", CallerId = "A", Recipient = "X", Currency = "GBP" });
            ctx.CdrRecords.Add(new CdrRecord { CallDate = d1, Duration = 20, Cost = 2m, Reference = "R2", CallerId = "B", Recipient = "Y", Currency = "GBP" });
            // Day2: one call
            ctx.CdrRecords.Add(new CdrRecord { CallDate = d2, Duration = 30, Cost = 3m, Reference = "R3", CallerId = "C", Recipient = "Z", Currency = "GBP" });

            await ctx.SaveChangesAsync();

            var svc = new CdrInsightsService(ctx);
            var summary = (await svc.GetDailySummaryAsync()).OrderBy(s => s.Date).ToList();

            Assert.Equal(2, summary.Count);
            Assert.Equal(d1, summary[0].Date);
            Assert.Equal(2, summary[0].TotalCalls);
            Assert.Equal(30, summary[0].TotalDuration);
            Assert.Equal(3m, summary[0].TotalCost);
            Assert.Equal(d2, summary[1].Date);
            Assert.Equal(1, summary[1].TotalCalls);
            Assert.Equal(30, summary[1].TotalDuration);
            Assert.Equal(3m, summary[1].TotalCost);
        }

        [Fact]
        public async Task GetCallCountInRangeAsync_ReturnsCorrectCount()
        {
            using var ctx = CreateInMemoryContext();
            var d1 = DateTime.Today;
            var d2 = d1.AddDays(1);
            ctx.CdrRecords.Add(new CdrRecord { CallDate = d1, Reference = "R1", CallerId = "A", Recipient = "X", Duration = 1, Cost = 1m, Currency = "GBP" });
            ctx.CdrRecords.Add(new CdrRecord { CallDate = d2, Reference = "R2", CallerId = "B", Recipient = "Y", Duration = 1, Cost = 1m, Currency = "GBP" });
            await ctx.SaveChangesAsync();

            var svc = new CdrInsightsService(ctx);
            var count = await svc.GetCallCountInRangeAsync(d1, d2);

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetTotalDurationByRecipientAsync_ReturnsCorrectSum()
        {
            using var ctx = CreateInMemoryContext();
            ctx.CdrRecords.AddRange(
                new CdrRecord { Recipient = "X", Duration = 10, Reference = "R1", CallerId = "A", CallDate = DateTime.Today, Cost = 1m, Currency = "GBP" },
                new CdrRecord { Recipient = "X", Duration = 15, Reference = "R2", CallerId = "B", CallDate = DateTime.Today, Cost = 1m, Currency = "GBP" },
                new CdrRecord { Recipient = "Y", Duration = 20, Reference = "R3", CallerId = "C", CallDate = DateTime.Today, Cost = 1m, Currency = "GBP" }
            );
            await ctx.SaveChangesAsync();

            var svc = new CdrInsightsService(ctx);
            var total = await svc.GetTotalDurationByRecipientAsync("X");

            Assert.Equal(25, total);
        }
        //Edge-Case Unit Tests.
        [Fact]
        public async Task GetAverageCallsPerDayAsync_WithNoRecords_ReturnsZero()
        {
            using var ctx = CreateInMemoryContext();
            var svc = new CdrInsightsService(ctx);

            var avg = await svc.GetAverageCallsPerDayAsync();

            Assert.Equal(0d, avg);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task GetTopCallersAsync_WithNonPositiveN_ReturnsEmpty(int n)
        {
            using var ctx = CreateInMemoryContext();
            // seed a couple records so we know the DB isn't empty
            ctx.CdrRecords.Add(new CdrRecord { CallerId = "A", Reference = "R1", Recipient = "X", CallDate = DateTime.Today, Duration = 1, Cost = 1m, Currency = "GBP" });
            await ctx.SaveChangesAsync();

            var svc = new CdrInsightsService(ctx);
            var top = await svc.GetTopCallersAsync(n);

            Assert.Empty(top);
        }

        [Fact]
        public async Task GetCallCountInRangeAsync_StartAfterEnd_ReturnsZero()
        {
            using var ctx = CreateInMemoryContext();
            // one record on today
            ctx.CdrRecords.Add(new CdrRecord { CallDate = DateTime.Today, Reference = "R1", CallerId = "A", Recipient = "X", Duration = 1, Cost = 1m, Currency = "GBP" });
            await ctx.SaveChangesAsync();

            var svc = new CdrInsightsService(ctx);
            // start > end
            var count = await svc.GetCallCountInRangeAsync(DateTime.Today.AddDays(1), DateTime.Today);

            Assert.Equal(0, count);
        }

        [Fact]
        public async Task GetTotalDurationByRecipientAsync_WithNoMatchingRecipient_ReturnsZero()
        {
            using var ctx = CreateInMemoryContext();
            ctx.CdrRecords.Add(new CdrRecord { Recipient = "X", Duration = 10, Reference = "R1", CallerId = "A", CallDate = DateTime.Today, Cost = 1m, Currency = "GBP" });
            await ctx.SaveChangesAsync();

            var svc = new CdrInsightsService(ctx);
            var total = await svc.GetTotalDurationByRecipientAsync("NON_EXISTENT");

            Assert.Equal(0, total);
        }
    }
}
