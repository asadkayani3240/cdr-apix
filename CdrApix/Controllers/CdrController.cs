using CdrApix.Data;
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

        [HttpGet("average-cost")]
        public async Task<ActionResult<decimal>> GetAverageCost()
        {
            return await _context.CdrRecords.AverageAsync(r => r.Cost);
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
    }

}
