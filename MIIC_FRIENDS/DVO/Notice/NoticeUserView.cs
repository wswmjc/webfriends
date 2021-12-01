using Miic.Friends.Common;
using Miic.Friends.Common.Setting;
using Miic.Friends.Notice.Setting;
using System.Collections.Generic;

namespace Miic.Friends.Notice
{
    public class NoticeUserView
    {
        public BusinessTypeSetting NoticeSource { get; set; }
        public NoticeTypeSetting NoticeType { get; set; }
        public List<SimpleUserView> Noticers { get; set; }
    }
}
