using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.General.Behavior
{
   
    public abstract class GeneralPraiseInfo
    {
        [MiicField(MiicStorageName = "ID", IsNotNull = true, IsPrimaryKey = true, MiicDbType = DbType.String, Description = "唯一码")]
        public string ID { get; set; }
        [MiicField(MiicStorageName = "PUBLISH_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "发布ID")]
        public string PublishID { get; set; }
        [MiicField(MiicStorageName = "PRAISER_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "点赞人ID")]
        public string PraiserID { get; set; }
        [MiicField(MiicStorageName = "PRAISER_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "点赞人名称")]
        public string PraiserName { get; set; }
        [MiicField(MiicStorageName = "PRAISE_TIME", IsNotNull = true, MiicDbType = DbType.DateTime, Description = "点赞时间")]
        public DateTime? PraiseTime { get; set; }
        [MiicField(MiicStorageName = "SORT_NO", IsIdentification = true, IsNotNull = true, MiicDbType = DbType.Int32, Description = "排序")]
        public int? SortNo { get; set; }
    }
}
