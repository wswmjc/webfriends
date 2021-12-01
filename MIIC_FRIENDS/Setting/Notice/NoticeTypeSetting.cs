using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Notice.Setting
{
    public enum NoticeTypeSetting
    {
        /// <summary>
        /// @发布信息
        /// </summary>
        [Description("@发布信息")]
        PublishInfo=0,
        /// <summary>
        /// @回复
        /// </summary>
        [Description("@回复")]
        Message=1
    }
}
