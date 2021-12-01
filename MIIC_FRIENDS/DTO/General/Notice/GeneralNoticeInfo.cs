using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.General.Notice
{
    public abstract  class GeneralNoticeInfo
    {

        [MiicField(MiicStorageName = "ID", IsNotNull = true, IsPrimaryKey = true, MiicDbType = DbType.String, Description = "唯一码")]
        public string ID { get; set; }
        [MiicField(MiicStorageName = "SOURCE", IsNotNull = true, MiicDbType = DbType.String, Description = "通知来源")]
        public string Source { get; set; }
        [MiicField(MiicStorageName = "PUBLISHER_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "发布者ID")]
        public string PublisherID { get; set; }
        [MiicField(MiicStorageName = "PUBLISHER_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "发布者名称")]
        public string PublisherName { get; set; }
        [MiicField(MiicStorageName = "PUBLISH_TIME", IsNotNull = true, MiicDbType = DbType.DateTime, Description = "发布时间")]
        public DateTime? PublishTime { get; set; }
        [MiicField(MiicStorageName = "NOTICER_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "通知者ID")]
        public string NoticerID { get; set; }
        [MiicField(MiicStorageName = "NOTICER_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "通知者名称")]
        public string NoticerName { get; set; }
        [MiicField(MiicStorageName = "READ_STATUS", IsNotNull = true, MiicDbType = DbType.String, Description = "阅读状态")]
        public string ReadStatus { get; set; }
        [MiicField(MiicStorageName = "READ_TIME", MiicDbType = DbType.DateTime, Description = "阅读时间")]
        public DateTime? ReadTime { get; set; }
    }
}
