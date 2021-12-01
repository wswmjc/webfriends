using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.General.SimpleGroup
{
    public abstract class GeneralSimpleGroupMember
    {
        [MiicField(MiicStorageName = "ID", IsNotNull = true, IsPrimaryKey = true, MiicDbType = DbType.String, Description = "唯一码")]
        public string ID { get; set; }
        [MiicField(MiicStorageName = "MEMBER_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "成员ID")]
        public string MemberID { get; set; }
        [MiicField(MiicStorageName = "MEMBER_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "成员名称")]
        public string MemberName { get; set; }
        [MiicField(MiicStorageName = "JOIN_TIME", IsNotNull = true, MiicDbType = DbType.DateTime, Description = "加入时间")]
        public DateTime? JoinTime { get; set; }
        [MiicField(MiicStorageName = "SORT_NO", IsNotNull = true, IsIdentification = true, MiicDbType = DbType.Int32, Description = "排序")]
        public int? SortNo { get; set; }

    }
}
