using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorpNumber.Models
{
    public class Nationalities
    {
        [Key]
        public int CodeNationality { get; set; }
        public string ? Nationality { get; set; }
        public string ?NationalityCh { get; set; }
    }

}
