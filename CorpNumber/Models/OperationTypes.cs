using System.ComponentModel.DataAnnotations;

namespace CorpNumber.Models
{
    public class OperationTypes
    {
        [Key]
        public int CodeOperType { get; set; }
        public string? Type { get; set; }

        public virtual ICollection<Operations>? Operations { get; set; }
    }
}
