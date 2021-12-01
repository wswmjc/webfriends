using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Moments.Behavior
{
    [MiicTable(MiicStorageName = "MOMENTS_PRAISE_INFO", Description = "朋友圈发点赞布表")]
    public partial class PraiseInfo : Miic.Friends.General.Behavior.GeneralPraiseInfo
    {
       
    }
}
