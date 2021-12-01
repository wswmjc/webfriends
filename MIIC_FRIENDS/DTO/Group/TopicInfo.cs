using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Group
{
    [MiicTable(MiicStorageName = "GROUP_TOPIC_INFO", Description = "讨论组讨论话题表")]
    public partial class TopicInfo : Miic.Friends.General.SimpleGroup.GeneralSimpleProjectInfo
    {
        [MiicField(MiicStorageName = "GROUP_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "讨论组ID")]
        public string GroupID { get; set; }
        [MiicField(MiicStorageName = "TOPIC_CONTENT", IsNotNull = true, MiicDbType = DbType.String, Description = "讨论组话题名称")]
        public string TopicContent { get; set; }
        [MiicField(MiicStorageName = "MESSAGE_COUNT", IsNotNull = true, MiicDbType = DbType.Int32, Description = "跟帖讨论数")]
        public int? MessageCount { get; set; }
    }
}
