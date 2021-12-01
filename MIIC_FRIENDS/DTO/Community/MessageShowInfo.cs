using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Community
{
    [MiicTable(MiicStorageName = "COMMUNITY_MESSAGE_SHOW_INFO", Description = "行业圈子讨论交流信息展示视图")]
    public partial class MessageShowInfo : Miic.Friends.General.SimpleGroup.GeneralSimpleMessageInfo
    {
        [MiicField(MiicStorageName = "FROM_COMMENTER_URL", IsNotNull = true, MiicDbType = DbType.String, Description = "FromUrl")]
        public string FromCommenterUrl { get; set; }
        [MiicField(MiicStorageName = "TO_COMMENTER_URL",  MiicDbType = DbType.String, Description = "ToUrl")]
        public string ToCommenterUrl { get; set; }

    }
}
