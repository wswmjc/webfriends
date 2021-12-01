using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.General.Behavior
{
    public abstract class GeneralTreadInfo
    {
        [MiicField(MiicStorageName = "ID", IsNotNull = true, IsPrimaryKey = true, MiicDbType = DbType.String, Description = "唯一码")]
        public string ID { get; set; }
        [MiicField(MiicStorageName = "PUBLISH_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "发布ID")]
        public string PublishID { get; set; }
        [MiicField(MiicStorageName = "TREADER_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "点踩者ID")]
        public string TreaderID { get; set; }
        [MiicField(MiicStorageName = "TREADER_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "点踩者名称")]
        public string TreaderName { get; set; }
        [MiicField(MiicStorageName = "TREAD_TIME", IsNotNull = true, MiicDbType = DbType.DateTime, Description = "点踩时间")]
        public DateTime? TreadTime { get; set; }
        [MiicField(MiicStorageName = "SORT_NO", IsNotNull = true, IsIdentification = true, MiicDbType = DbType.Int32, Description = "排序")]
        public int? SortNo { get; set; }
    }
}
