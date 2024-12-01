using AlgoTradingKiteApi.Models;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AlgoTradingKiteApi.Services
{
    public class CSVHelper
    {
        public List<OptionData> DeserializeCsv(string csvContent)
        {
            // Create a StringReader to read the CSV content
            using (var reader = new StringReader(csvContent))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
               

                // Read the records and convert to a list of OptionData objects
                var records = csv.GetRecords<OptionData>().ToList();

                return records;
            }
        }
    }
}
