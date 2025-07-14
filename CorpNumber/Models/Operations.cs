using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorpNumber.Models
{
    public class Operations
    {
        [Key]
        public int CodeOperation { get; set; }

        public DateTime? RequestDate { get; set; }
        public DateTime? OperDate { get; set; }

        public int? CodeOperType { get; set; }
        public int? Number { get; set; }

        public string? ICCID_old { get; set; }
        public string? ICCID_new { get; set; }

        public int? Owner_old { get; set; }
        public int? Owner_new { get; set; }

        public int? Account_old { get; set; }
        public int? Account_new { get; set; }

        public int? Status_old { get; set; }
        public int? Status_new { get; set; }

        public int? Internet_old { get; set; }
        public int? Internet_new { get; set; }

        public short? Limit_old { get; set; }
        public short? Limit_new { get; set; }

        public int? Tariff_old { get; set; }
        public int? Tariff_new { get; set; }

        public bool? Complete { get; set; }

        public string? Comments { get; set; }
        public string? Information { get; set; }

        public int? OrderN { get; set; }

        [Timestamp]
        public byte[]? SSMA_TimeStamp { get; set; }

        // Навигационное свойство (если нужно)
        public virtual OperationTypes? OperationTypes { get; set; }
        public virtual Phone? Phone { get; set; }


        [ForeignKey("Owner_old")]
        public virtual Owner? OwnerOld { get; set; }

        [ForeignKey("Owner_new")]
        public virtual Owner? OwnerNew { get; set; }

    }
}
