using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Common.Setting
{
    /// <summary>
    /// 业务类别枚举
    /// </summary>
    public enum BusinessTypeSetting
    {
        /// <summary>
        /// 讨论组
        /// </summary>
        [Description("讨论组")]
        Group = 2,
        /// <summary>
        /// 行业圈子
        /// </summary>
        [Description("行业圈子")]
        Community = 1,
        /// <summary>
        /// 朋友圈
        /// </summary>
        [Description("朋友圈")]
        Moments = 0
    }
}
