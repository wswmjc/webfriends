using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.General
{
    public abstract class GeneralBrowseInfo
    {
        [MiicField(MiicStorageName = "ID", IsNotNull = true, IsPrimaryKey = true, MiicDbType = DbType.String, Description = "唯一码")]
        public string ID { get; set; }
        [MiicField(MiicStorageName = "PUBLISH_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "发布ID")]
        public string PublishID { get; set; }
        [MiicField(MiicStorageName = "BROWSER_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "浏览者ID")]
        public string BrowserID { get; set; }
        [MiicField(MiicStorageName = "BROWSER_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "浏览者名称")]
        public string BrowserName { get; set; }
        [MiicField(MiicStorageName = "BROWSE_TIME", IsNotNull = true, MiicDbType = DbType.DateTime, Description = "浏览时间")]
        public DateTime? BrowseTime { get; set; }
        [MiicField(MiicStorageName = "IS_HINTED", IsNotNull = true, MiicDbType = DbType.Int32, Description = "是否设置失效")]
        public string IsHinted { get; set; }
        [MiicField(MiicStorageName = "SORT_NO", IsNotNull = true, IsIdentification = true, MiicDbType = DbType.Int32, Description = "排序")]
        public int? SortNo { get; set; }
        [MiicField(MiicStorageName = "BROWSER_IP", IsNotNull = true, MiicDbType = DbType.String, Description = "浏览人IP")]
        public string BrowserIP { get; set; }
    }
}
