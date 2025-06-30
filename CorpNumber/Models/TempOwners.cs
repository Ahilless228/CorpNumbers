using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CorpNumber.Models
{
    public class TempOwners
    {
        [Key]
        public int CodeTempOwner { get; set; }
        public string? NameTO { get; set; }
        public string? NameTOCh { get; set; }
        public string? Organization { get; set; }
        public int? HostDepartment { get; set; }
        public int? Officer { get; set; }
        public short? Period { get; set; }

    }
}
