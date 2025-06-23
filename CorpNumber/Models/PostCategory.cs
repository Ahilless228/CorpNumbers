using System.ComponentModel.DataAnnotations;

namespace CorpNumber.Models
{
    public class PostCategory
    {
        [Key]
        public int Code { get; set; }
        public string Category { get; set; }
        public string CategoryCh { get; set; }
    }

}
