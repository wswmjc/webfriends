using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Miic.Friends.Common
{
    public class ToCommentView:CommentView
    {
        public string ToCommenterID { get; set; }
        public string ToCommenterName { get; set; }
    }
}
