using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Community
{
    [MiicTable(MiicStorageName = "COMMUNITY_INFO", Description = "圈子表")]
    public partial class CommunityInfo : Miic.Friends.General.SimpleGroup.GeneralSimpleGroupInfo
    {

        [MiicField(MiicStorageName = "COMMUNITY_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "圈子名称")]
        public string Name { get; set; }
        [MiicField(MiicStorageName = "MEMBER_COUNT", IsNotNull = true, MiicDbType = DbType.Int32, Description = "成员数目")]
        public int? MemberCount { get; set; }
        [MiicField(MiicStorageName = "VALID", IsNotNull = true, MiicDbType = DbType.String, Description = "有效性")]
        public string Valid { get; set; }
        [MiicField(MiicStorageName = "END_TIME", MiicDbType = DbType.DateTime, Description = "失效时间")]
        public DateTime? EndTime { get; set; }
        [MiicField(MiicStorageName = "REMARK", MiicDbType = DbType.String, Description = "圈子备注")]
        public string Remark { get; set; }
          [MiicField(MiicStorageName = "CAN_SEARCH", MiicDbType = DbType.String, Description = "是否能搜索")]
        public string CanSearch { get; set; }
    }
}
