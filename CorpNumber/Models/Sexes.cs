using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorpNumber.Models
{
    public class Sexes
    {
        [Key]
        public int CodeSex { get; set; }
        public string ?Sex { get; set; }
        public string ?SexCh { get; set; }
    }
}
