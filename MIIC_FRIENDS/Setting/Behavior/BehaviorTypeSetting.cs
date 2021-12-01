using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Behavior.Setting
{
    public enum BehaviorTypeSetting
    {
        [Description("赞")]
        Praise = 0,
        [Description("取消赞")]
        CancelPraise = 6,
        [Description("踩")]
        Tread = 1,
        [Description("取消踩")]
        CancelTread = 7,
        [Description("举报")]
        Report = 2,
        [Description("收藏")]
        Collect = 4,
        [Description("取消收藏")]
        CancelCollect = 5,
        [Description("评论")]
        Comment = 3,
        [Description("取消评论")]
        CancelComment = 9
    }
}
