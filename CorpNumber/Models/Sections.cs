using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorpNumber.Models
{
    public class Sections
    {
        [Key]
        public int CodeSection { get; set; }
        public string? Section { get; set; }
        public string? SectionCh { get; set; }
        public int? Department { get; set; }
        public bool? Actuality { get; set; }
        public byte[]? SSMA_TimeStamp { get; set; }

        // Связь с Department

        [ForeignKey("Department")]
        public Department? DepartmentNavigation { get; set; }

    }

}
