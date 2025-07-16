using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParsingPDF
{
    public class ServiceCharge
    {
        public int Id { get; set; }
        public int ServiceNameId { get; set; }
        public ServiceName ServiceName { get; set; }
        public decimal Amount { get; set; }
        public int ReportEntryId { get; set; }
        public ReportEntry ReportEntry { get; set; }
    }
}