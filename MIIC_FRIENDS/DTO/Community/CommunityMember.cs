using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Community
{
    [MiicTable(MiicStorageName = "COMMUNITY_MEMBER", Description = "圈子成员表")]
    public partial class CommunityMember : Miic.Friends.General.SimpleGroup.GeneralSimpleGroupMember
    {
        [MiicField(MiicStorageName = "COMMUNITY_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "圈子ID")]
        public string CommunityID { get; set; }

        [MiicField(MiicStorageName = "IS_ADMIN", IsNotNull = true, MiicDbType = DbType.String, Description = "是否为管理员")]
        public string IsAdmin { get; set; }
    }
}
