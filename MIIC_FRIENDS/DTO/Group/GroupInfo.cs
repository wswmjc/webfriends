using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Group
{
    [MiicTable(MiicStorageName = "GROUP_INFO", Description = "讨论组表")]
    public partial class GroupInfo:Miic.Friends.General.SimpleGroup.GeneralSimpleProjectInfo
    {
      
        [MiicField(MiicStorageName = "GROUP_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "讨论组名称")]
        public string Name { get; set; }
        [MiicField(MiicStorageName = "LOGO_URL", MiicDbType = DbType.String, Description = "讨论组头像")]
        public string LogoUrl { get; set; }
        [MiicField(MiicStorageName = "MEMBER_COUNT", IsNotNull = true, MiicDbType = DbType.Int32, Description = "成员数")]
        public int? MemberCount { get; set; }

    }
}
