using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorpNumber.Models
{
    public class Section
    {
        [Key]
        public int CodeSection { get; set; }
        [Column("Section")]
        public string? SectionName { get; set; }
        public string? SectionCh { get; set; }
        public int? Department { get; set; }
        public bool? Actuality { get; set; }
        public byte[]? SSMA_TimeStamp { get; set; }
    }

}
