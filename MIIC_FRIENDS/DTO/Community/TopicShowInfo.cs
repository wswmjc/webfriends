using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Community
{
    [MiicTable(MiicStorageName = "COMMUNITY_TOPIC_SHOW_INFO", Description = "行业圈子讨论话题视图")]
    public partial class TopicShowInfo : Miic.Friends.General.SimpleGroup.GeneralSimpleProjectInfo
    {
        [MiicField(MiicStorageName = "COMMUNITY_ID", MiicDbType = DbType.String, Description = "圈子ID")]
        public string CommunityID { get; set; }
        [MiicField(MiicStorageName = "TOPIC_CONTENT",  MiicDbType = DbType.String, Description = "话题内容")]
        public string TopicContent { get; set; }
        [MiicField(MiicStorageName = "MESSAGE_COUNT",  MiicDbType = DbType.Int32, Description = "回复数目")]
        public int? MessageCount { get; set; }
        [MiicField(MiicStorageName = "CREATER_URL",  MiicDbType = DbType.String, Description = "创建者URL")]
        public string CreaterUrl { get; set; }
    }
}
