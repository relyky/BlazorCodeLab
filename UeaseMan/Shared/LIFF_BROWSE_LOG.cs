namespace UeaseMan.Shared
{
    using System;
    //using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    //using System.ComponentModel.DataAnnotations.Schema;
    //using System.Data.Entity.Spatial;
    //using Dapper.Contrib.Extensions;
    using KeyAttribute = Dapper.Contrib.Extensions.KeyAttribute;
    using TableAttribute = Dapper.Contrib.Extensions.TableAttribute;

    [Table("LIFF_BROWSE_LOG")]
    public class LIFF_BROWSE_LOG
    {
        [Key]
        public long sn { get; set; }
        public DateTime browseDtm { get; set; }
        [Required]
        public string lineId { get; set; }
        [Required]
        public string lineDisplayName { get; set; }
        [Required]
        public string productId { get; set; }
        [Required]
        public string productName { get; set; }
    }
}
