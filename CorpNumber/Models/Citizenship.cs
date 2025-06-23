using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CorpNumber.Models
    

{
    public class Citizenship
    {
        [Key]
        public int CodeCitizenship { get; set; }
        [Column("Citizenship")]
        public string ?Citizenshipp { get; set; }
        public string ?CitizenshipCh { get; set; }
    }

}
