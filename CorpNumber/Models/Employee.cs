using System.ComponentModel.DataAnnotations;

namespace CorpNumber.Models
{
    public class Employee
    {
        [Key]
        public int CodeEmployee { get; set; }
        public string? Surname { get; set; }
        public string? Firstname { get; set; }
        public string? Midname { get; set; }
        public string? NameCh { get; set; }
        public int? TabNum { get; set; }
        public int? Post { get; set; }
        public int? PartTime { get; set; }
        public int? Department { get; set; }
        public int? Section { get; set; }
        public DateTime? InputDate { get; set; }
        public string? ContractNumber { get; set; }
        public DateTime? ContractDate { get; set; }
        public byte? Hazard { get; set; }
        public int? HazardDoc { get; set; }
        public int? CodeQuota { get; set; }
        public bool? Fired { get; set; }
        public DateTime? FiringDate { get; set; }
        public int? Sex { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Passport { get; set; }
        public string? Address { get; set; }
        public int? District { get; set; }
        public int? Citizenship { get; set; }
        public int? Nationality { get; set; }
        public byte[]? SSMA_TimeStamp { get; set; }
    }
}
