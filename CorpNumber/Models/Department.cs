using System.ComponentModel.DataAnnotations;

namespace CorpNumber.Models
{
    public class Department
    {
        [Key]
        public int CodeDepartment { get; set; }
        public string? DepartmentName { get; set; }
        public string? DepartmentCh { get; set; }
        public bool? Actuality { get; set; }
        public byte[]? SSMA_TimeStamp { get; set; }
    }
}
