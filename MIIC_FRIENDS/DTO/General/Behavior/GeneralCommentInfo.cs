using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.General.Behavior
{
   
    public abstract class GeneralCommentInfo
    {
        [MiicField(MiicStorageName = "ID", IsNotNull = true, IsPrimaryKey = true, MiicDbType = DbType.String, Description = "唯一码")]
        public string ID { get; set; }
       
        [MiicField(MiicStorageName = "FROM_COMMENTER_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "发布者ID")]
        public string FromCommenterID { get; set; }
        [MiicField(MiicStorageName = "FROM_COMMENTER_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "发布者名称")]
        public string FromCommenterName { get; set; }
        [MiicField(MiicStorageName = "TO_COMMENTER_ID", MiicDbType = DbType.String, Description = "接受者ID")]
        public string ToCommenterID { get; set; }
        [MiicField(MiicStorageName = "TO_COMMENTER_NAME",MiicDbType = DbType.String, Description = "接受者名称")]
        public string ToCommenterName { get; set; }
        [MiicField(MiicStorageName = "COMMENT_TIME", IsNotNull = true, MiicDbType = DbType.DateTime, Description = "发布时间")]
        public DateTime? CommentTime { get; set; }
        [MiicField(MiicStorageName = "SORT_NO", IsNotNull = true, IsIdentification=true, MiicDbType = DbType.Int32, Description = "排序")]
        public int? SortNo { get; set; }
    }
}
