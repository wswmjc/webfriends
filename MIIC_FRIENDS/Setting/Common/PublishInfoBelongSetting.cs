using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Common.Setting
{
    public enum PublishInfoBelongSetting
    {
        [Description("网站首页")]
        Main = 0,
        [Description("个人主页")]
        Self = 1,
        [Description("他人主页")]
        Other = 2
    }
}
