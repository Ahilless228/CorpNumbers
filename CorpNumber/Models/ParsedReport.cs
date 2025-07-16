using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserAPI.Models
{
    public class ParsedReport
    {
        public string PhoneNumber { get; set; }
        public string ICCID { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime ReportDate { get; set; }
        public List<(string ServiceName, decimal Amount)> Services { get; set; } = new();
    }
}