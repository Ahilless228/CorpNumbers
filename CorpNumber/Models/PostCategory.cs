using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorpNumber.Models
{
    public class PostCategory
    {
        [Key]
        public int Code { get; set; }
        public string? Category { get; set; }
        public string? CategoryCh { get; set; }

        

    }

}
