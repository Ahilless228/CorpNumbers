using CorpNumber.Models;
using ParsingPDF;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

public class ReportEntry
{
    public int Id { get; set; }
    public int PhoneId { get; set; }

    [NotMapped]
    public Phone Phone { get; set; }

    public decimal TotalAmount { get; set; }
    public DateTime ReportDate { get; set; }
    public List<ServiceCharge> Services { get; set; } = new();
}

