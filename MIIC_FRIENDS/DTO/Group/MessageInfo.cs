using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Group
{
    [MiicTable(MiicStorageName = "GROUP_MESSAGE_INFO", Description = "讨论组交流信息表")]
    public partial class MessageInfo : Miic.Friends.General.SimpleGroup.GeneralSimpleMessageInfo
    {
       
       
    }
}
