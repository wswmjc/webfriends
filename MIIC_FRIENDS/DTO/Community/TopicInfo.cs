using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Community
{
    [MiicTable(MiicStorageName = "COMMUNITY_TOPIC_INFO", Description = "行业圈子讨论话题表")]
    public partial class TopicInfo : Miic.Friends.General.SimpleGroup.GeneralSimpleProjectInfo
    {
        [MiicField(MiicStorageName = "COMMUNITY_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "圈子ID")]
        public string CommunityID { get; set; }
        [MiicField(MiicStorageName = "TOPIC_CONTENT", IsNotNull = true, MiicDbType = DbType.String, Description = "话题内容")]
        public string TopicContent { get; set; }
        [MiicField(MiicStorageName = "MESSAGE_COUNT", IsNotNull = true, MiicDbType = DbType.Int32, Description = "回复数目")]
        public int? MessageCount { get; set; }
    }
}
