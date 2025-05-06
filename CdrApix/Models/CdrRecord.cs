using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CdrApix.Models
{
    [Table("CdrRecords")]
    public class CdrRecord
    {
        [Key]
        public string Reference { get; set; }
        public string CallerId { get; set; }
        public string Recipient { get; set; }
        public DateTime CallDate { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Duration { get; set; }
        [Column(TypeName = "decimal(18,3)")]
        public decimal Cost { get; set; }
        public string Currency { get; set; }
    }
}
