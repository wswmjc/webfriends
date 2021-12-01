using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Notice
{
    [MiicTable(MiicStorageName = "MESSAGE_INFO", Description = "消息通知表")]
    public class MessageInfo : Miic.Friends.General.Notice.GeneralNoticeInfo
    {

        [MiicField(MiicStorageName = "PUBLISH_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "信息发布ID")]
        public string PublishID { get; set; }

        [MiicField(MiicStorageName = "MESSAGE_TYPE", IsNotNull = true, MiicDbType = DbType.String, Description = "消息类型")]
        public string MessageType { get; set; }

    }
}
