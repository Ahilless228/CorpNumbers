using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CorpNumber.Models
{
    public class Department
    {
        [Key]
        public int CodeDepartment { get; set; }
        [Column("Department")]
        public string? DepartmentName { get; set; }
        public string? DepartmentCh { get; set; }
        public bool? Actuality { get; set; }
        public byte[]? SSMA_TimeStamp { get; set; }
    }
}
