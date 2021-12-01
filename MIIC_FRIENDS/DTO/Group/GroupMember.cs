using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Group
{
    [MiicTable(MiicStorageName = "GROUP_MEMBER", Description = "讨论组成员表")]
    public partial class GroupMember : Miic.Friends.General.SimpleGroup.GeneralSimpleGroupMember
    {

        [MiicField(MiicStorageName = "GROUP_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "讨论组ID")]
        public string GroupID { get; set; }
        [MiicField(MiicStorageName = "REMARK", MiicDbType = DbType.String, Description = "备注")]
        public string Remark { get; set; }


    }
}
