using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Community.Behavior
{
    [MiicTable(MiicStorageName = "COMMUNITY_PRAISE_INFO", Description = "圈子发点赞布表")]
    public partial class PraiseInfo:Miic.Friends.General.Behavior.GeneralPraiseInfo
    {
      
    }
}
