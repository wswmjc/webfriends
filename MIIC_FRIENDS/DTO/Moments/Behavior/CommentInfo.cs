using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Moments.Behavior
{
    [MiicTable(MiicStorageName = "MOMENTS_COMMENT_INFO", Description = "朋友圈评论表")]
    public partial class CommentInfo:Miic.Friends.General.Behavior.GeneralCommentInfo
    {
        [MiicField(MiicStorageName = "PUBLISH_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "发布ID")]
        public string PublishID { get; set; }
        [MiicField(MiicStorageName = "COMMENT_CONTENT", IsNotNull = true, MiicDbType = DbType.String, Description = "内容")]
        public string Content { get; set; } 
    }
}
