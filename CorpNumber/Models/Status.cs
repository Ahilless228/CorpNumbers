using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorpNumber.Models
{
    public class Status
    {
        [Key]
        public int Code { get; set; }
        [Column("Status")]
        public string ?StatusText { get; set; }

        public virtual ICollection<Phone>? Phones { get; set; }
    }   
}
