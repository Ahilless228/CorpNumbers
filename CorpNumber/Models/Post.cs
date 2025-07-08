using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorpNumber.Models
{
    public class Post
    {
        [Key]
        public int CodePost { get; set; }
        [Column("Post")]
        public string ?Postt { get; set; }
        public string ?PostCh { get; set; }
        public int? Category { get; set; }

        [ForeignKey("Category")]
        public PostCategory? PostCategoryNavigation { get; set; }
    }

}
