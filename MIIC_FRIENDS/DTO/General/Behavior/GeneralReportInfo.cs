using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.General.Behavior
{
    public abstract class GeneralReportInfo
    {
        [MiicField(MiicStorageName = "ID", IsNotNull = true, IsPrimaryKey = true, MiicDbType = DbType.String, Description = "唯一码")]
        public string ID { get; set; }
        [MiicField(MiicStorageName = "PUBLISH_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "信息发布ID")]
        public string PublishID { get; set; }
        [MiicField(MiicStorageName = "REPORTER_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "举报人ID")]
        public string ReporterID { get; set; }
        [MiicField(MiicStorageName = "REPORTER_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "举报名称")]
        public string ReporterName { get; set; }
        [MiicField(MiicStorageName = "REPORT_TIME", IsNotNull = true, MiicDbType = DbType.DateTime, Description = "举报时间")]
        public DateTime? ReportTime { get; set; }
        [MiicField(MiicStorageName = "REPORT_STATUS", IsNotNull = true, MiicDbType = DbType.String, Description = "举报状态(默认未受理2)")]
        public string ReportStatus { get; set; }
        [MiicField(MiicStorageName = "SORT_NO", IsNotNull = true, IsIdentification = true, MiicDbType = DbType.Int32, Description = "排序")]
        public int? SortNo { get; set; }
    }
}
