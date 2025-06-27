using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CorpNumber.Models
    

{
    public class Citizenships
    {
        [Key]
        public int CodeCitizenship { get; set; }
        public string ?Citizenship { get; set; }
        public string ?CitizenshipCh { get; set; }
    }

}
