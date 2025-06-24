using System.ComponentModel.DataAnnotations;

namespace CorpNumber.Models
{
    public class OwnerCategory
    {
        [Key]
        public int CodeCategory { get; set; }
        public string? Category { get; set; }
        public string? CategoryCh { get; set; }
        public bool? Others { get; set; }

        public virtual ICollection<Owner>? Owners { get; set; }
    }
}
