using CsvHelper.Configuration;
namespace CdrApix.Models
{
    public class CdrRecordMap : ClassMap<CdrRecord>
    {
        public CdrRecordMap()
        {
            Map(m => m.Reference).Name("reference");
            Map(m => m.CallerId).Name("caller_id");
            Map(m => m.Recipient).Name("recipient");
            Map(m => m.CallDate)
               .Name("call_date")
               .TypeConverterOption
               .DateTimeStyles(System.Globalization.DateTimeStyles.None)
               .TypeConverterOption
               .Format("dd/MM/yyyy");

            Map(m => m.EndTime)
      .Name("end_time")
      .TypeConverterOption
      .Format("hh\\:mm\\:ss");

            Map(m => m.Duration).Name("duration");
            Map(m => m.Cost).Name("cost");
            Map(m => m.Currency).Name("currency");
        }
    }
}
