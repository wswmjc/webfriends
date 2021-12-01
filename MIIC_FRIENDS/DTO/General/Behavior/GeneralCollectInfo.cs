using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.General.Behavior
{
    public abstract class GeneralCollectInfo
    {
        [MiicField(MiicStorageName = "ID", IsNotNull = true, IsPrimaryKey = true, MiicDbType = DbType.String, Description = "唯一码")]
        public string ID { get; set; }
        [MiicField(MiicStorageName = "PUBLISH_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "微博发布ID")]
        public string PublishID { get; set; }
        [MiicField(MiicStorageName = "COLLECTOR_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "收藏人ID")]
        public string CollectorID { get; set; }
        [MiicField(MiicStorageName = "COLLECTOR_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "收藏人名称")]
        public string CollectorName { get; set; }
        [MiicField(MiicStorageName = "COLLECT_TIME", IsNotNull = true, MiicDbType = DbType.DateTime, Description = "收藏时间")]
        public DateTime? CollectTime { get; set; }
        [MiicField(MiicStorageName = "COLLECT_VALID", IsNotNull = true, MiicDbType = DbType.String, Description = "有效标志")]
        public string CollectValid { get; set; }
        [MiicField(MiicStorageName = "SORT_NO", IsNotNull = true, IsIdentification = true, MiicDbType = DbType.Int32, Description = "排序")]
        public int? SortNo { get; set; }
    }
}
