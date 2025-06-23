using System.ComponentModel.DataAnnotations.Schema;

namespace CorpNumber.Models
{
    public class Post
    {
        public int CodePost { get; set; }
        [Column("Post")]
        public string Postt { get; set; }
        public string PostCh { get; set; }
        public int? Category { get; set; }
    }

}
