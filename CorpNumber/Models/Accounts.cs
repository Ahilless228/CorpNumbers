using System.ComponentModel.DataAnnotations;

namespace CorpNumber.Models;
public class Accounts
{
    [Key]
    public int Code { get; set; }
    public string ?Type { get; set; }
}
