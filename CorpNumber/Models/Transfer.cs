namespace CorpNumber.Models
{
    public class Transfer
    {
        public int CodeTransfer { get; set; }
        public int? ProcType { get; set; }
        public int? CodeEmp { get; set; }
        public int? TabNum { get; set; }
        public DateTime? TransferDate { get; set; }
        public int? CurPost { get; set; }
        public int? CurDepartment { get; set; }
        public int? CurSection { get; set; }
        public int? CurQuota { get; set; }
        public int? NewPost { get; set; }
        public int? NewDepartment { get; set; }
        public int? NewSection { get; set; }
        public int? NewQuota { get; set; }
        public string Reason { get; set; }
        public string OrderNumber { get; set; }
        public DateTime? OrderDate { get; set; }
        public int? Article { get; set; }
        public string Comments { get; set; }
    }

}
