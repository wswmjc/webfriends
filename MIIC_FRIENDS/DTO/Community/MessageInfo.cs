using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Community
{
    [MiicTable(MiicStorageName = "COMMUNITY_MESSAGE_INFO", Description = "圈子交流信息表")]
    public partial class MessageInfo : Miic.Friends.General.SimpleGroup.GeneralSimpleMessageInfo
    {
      
    }
}
