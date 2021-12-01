using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Notice
{
    [MiicTable(MiicStorageName = "NOTICE_INFO", Description = "@通知表")]
    public partial class NoticeInfo : Miic.Friends.General.Notice.GeneralNoticeInfo
    {

        [MiicField(MiicStorageName = "PUBLISH_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "信息发布ID")]
        public string PublishID { get; set; }

        [MiicField(MiicStorageName = "NOTICE_TYPE", IsNotNull = true, MiicDbType = DbType.String, Description = "通知类型")]
        public string NoticeType { get; set; }
        [MiicField(MiicStorageName = "COMMENT_ID", MiicDbType = DbType.String, Description = "如果通知为回复则填写回复ID")]
        public string CommentID { get; set; }
        

    }
}
