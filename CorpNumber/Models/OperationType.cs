using System.ComponentModel.DataAnnotations;

namespace CorpNumber.Models
{
    public class OperationType
    {
        [Key]
        public int CodeOperType { get; set; }
        public string? Type { get; set; }

        public virtual ICollection<Operation>? Operations { get; set; }
    }
}
