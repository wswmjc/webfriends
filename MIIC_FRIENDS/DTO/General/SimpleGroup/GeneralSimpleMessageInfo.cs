using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.General.SimpleGroup
{
    public abstract class GeneralSimpleMessageInfo : Miic.Friends.General.Behavior.GeneralCommentInfo
    {
        [MiicField(MiicStorageName = "TOPIC_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "话题ID")]
        public string TopicID { get; set; }

        [MiicField(MiicStorageName = "MESSAGE_CONTENT", IsNotNull = true, MiicDbType = DbType.String, Description = "讨论内容")]
        public string Content { get; set; }
       
    }
}
