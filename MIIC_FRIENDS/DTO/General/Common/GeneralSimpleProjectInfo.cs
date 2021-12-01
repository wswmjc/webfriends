using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.General.SimpleGroup
{
    public abstract class GeneralSimpleProjectInfo
    {
        [MiicField(MiicStorageName = "ID", IsNotNull = true, IsPrimaryKey = true, MiicDbType = DbType.String, Description = "唯一码")]
        public string ID { get; set; }
        [MiicField(MiicStorageName = "CREATER_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "创建人ID")]
        public string CreaterID { get; set; }
        [MiicField(MiicStorageName = "CREATER_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "创建人")]
        public string CreaterName { get; set; }
        [MiicField(MiicStorageName = "CREATE_TIME", IsNotNull = true, MiicDbType = DbType.DateTime, Description = "创建时间")]
        public DateTime? CreateTime { get; set; }
        [MiicField(MiicStorageName = "VALID", IsNotNull = true, MiicDbType = DbType.String, Description = "有效性")]
        public string Valid { get; set; }
        [MiicField(MiicStorageName = "END_TIME", MiicDbType = DbType.DateTime, Description = "失效时间")]
        public DateTime? EndTime { get; set; }
        [MiicField(MiicStorageName = "SORT_NO", IsIdentification = true, IsNotNull = true, MiicDbType = DbType.Int32, Description = "排序")]
        public int? SortNo { get; set; }
    }
}
