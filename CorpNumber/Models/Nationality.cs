using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorpNumber.Models
{
    public class Nationality
    {
        [Key]
        public int CodeNationality { get; set; }
        [Column("Nationality")]
        public string Nation { get; set; }
        public string NationalityCh { get; set; }
    }

}
