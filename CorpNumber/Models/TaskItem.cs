using System.ComponentModel.DataAnnotations;

namespace CorpNumber.Models
{
    public class TaskItem
    {
        [Key]
        public int CodeTask { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? TaskDate { get; set; }
        public string? TaskText { get; set; }
        public bool? Complete { get; set; }
        [Timestamp]
        public byte[]? SSMA_TimeStamp { get; set; }
    }

}
