namespace CdrApix.DTOs
{
    public class CallSummaryDto
    {
        public DateTime Date { get; set; }
        public int TotalCalls { get; set; }
        public int TotalDuration { get; set; }
        public decimal TotalCost { get; set; }
    }
}
