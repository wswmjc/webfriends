using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Community
{
    [MiicTable(MiicStorageName = "COMMUNITY_SEARCH_INFO", Description = "圈子查询视图")]
    public class CommunitySearchInfo : CommunityInfo
    {
        [MiicField(MiicStorageName = "TOPIC_COUNT", MiicDbType = DbType.Int32, Description = "话题数目")]
        public int? TopicCount { get; set; }
        [MiicField(MiicStorageName = "MESSAGE_COUNT", MiicDbType = DbType.Int32, Description = "话题讨论数目")]
        public int? MessageCount { get; set; }
        [MiicField(MiicStorageName = "MEMBER_ID", MiicDbType = DbType.String, Description = "成员ID")]
        public string MemberID { get; set; }
        [MiicField(MiicStorageName = "MEMBER_NAME", MiicDbType = DbType.String, Description = "成员姓名")]
        public string MemberName { get; set; }
        [MiicField(MiicStorageName = "IS_ADMIN", MiicDbType = DbType.String, Description = "是否是创建人")]
        public string IsAdmin { get; set; }
        [MiicField(MiicStorageName = "JOIN_TIME", MiicDbType = DbType.String, Description = "加入时间")]
        public DateTime? JoinTime { get; set; }
        [MiicField(MiicStorageName = "COMMUNITY_MEMBER_ID", MiicDbType = DbType.String, Description = "成员表ID")]
        public string CommunityMemberID { get; set; }
    }
}
