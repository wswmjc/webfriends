using Miic.Attribute;
using System;
using System.Data;

namespace Miic.Friends.Community
{
    [MiicTable(MiicStorageName = "COMMUNITY_STATISTICS", Description = "圈子统计表表")]
    public class CommunityStatistics
    {
        [MiicField(MiicStorageName = "ID", MiicDbType = DbType.String, Description = "圈子ID")]
        public string ID { get; set; }
        [MiicField(MiicStorageName = "COMMUNITY_NAME", MiicDbType = DbType.String, Description = "圈子名称")]
        public string CommunityName { get; set; }
        [MiicField(MiicStorageName = "MEMBER_ID",  MiicDbType = DbType.String, Description = "成员ID")]
        public string MemberID { get; set; }
        [MiicField(MiicStorageName = "MEMBER_NAME",  MiicDbType = DbType.String, Description = "成员名称")]
        public string MemberName { get; set; }
        [MiicField(MiicStorageName = "TOPIC_COUNT",  MiicDbType = DbType.Int32, Description = "讨论数")]
        public int? TopicCount { get; set; }
        [MiicField(MiicStorageName = "PUBLISH_COUNT",  MiicDbType = DbType.Int32, Description = "圈子信息数")]
        public int? PublishCount { get; set; }
    }
}
