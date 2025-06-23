namespace CorpNumber.Models
{
    public class TaskItem
    {
        public int CodeTask { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? TaskDate { get; set; }
        public string TaskText { get; set; }
        public bool? Complete { get; set; }
        public byte[] SSMA_TimeStamp { get; set; }
    }

}
