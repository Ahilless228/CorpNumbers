using System.ComponentModel.DataAnnotations;

namespace CorpNumber.Models;

public class Articles
{
    [Key]
    public int CodeArticle { get; set; }
    public string Number { get; set; }
    public string Information { get; set; }
}
