using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Community.Setting
{
    public enum ApplyStatusSetting
    {
        /// <summary>
        /// 邀请
        /// </summary>
        [Description("邀请")]
        Invite = 0,
        /// <summary>
        /// 申请
        /// </summary>
        [Description("申请")]
        Apply = 1,
        /// <summary>
        /// 同意
        /// </summary>
        [Description("同意")]
        Agree = 2,
        /// <summary>
        /// 拒绝
        /// </summary>
        [Description("拒绝")]
        Refuse = 3,
        /// <summary>
        /// 忽略
        /// </summary>
        [Description("忽略")]
        Ignore = 4
    }
}
