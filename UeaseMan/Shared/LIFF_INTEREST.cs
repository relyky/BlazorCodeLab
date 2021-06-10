namespace UeaseMan.Shared
{
    using System;
    //using System.Collections.Generic;
    //using System.ComponentModel.DataAnnotations;
    //using System.ComponentModel.DataAnnotations.Schema;
    //using System.Data.Entity.Spatial;
    //using Dapper.Contrib.Extensions;
    using KeyAttribute = Dapper.Contrib.Extensions.KeyAttribute;
    using TableAttribute = Dapper.Contrib.Extensions.TableAttribute;

    [Table("LIFF_INTEREST")]
    public class LIFF_INTEREST
    {
        [Key]
        public long sn { get; set; }
        public string ctxName { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string productId { get; set; }
        public string productName { get; set; }
        public string lineId { get; set; }
        public string lineDisplayName { get; set; }
        public string salesId { get; set; }
        public DateTime insertDtm { get; set; }
    }
}
