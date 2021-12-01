using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.General.SimpleGroup
{
    public abstract class GeneralSimpleGroupInfo
    {
        [MiicField(MiicStorageName = "ID", IsNotNull = true, IsPrimaryKey = true, MiicDbType = DbType.String, Description = "唯一码")]
        public string ID { get; set; }
        [MiicField(MiicStorageName = "CREATER_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "创建者ID")]
        public string CreaterID { get; set; }
        [MiicField(MiicStorageName = "CREATER_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "创建者名称")]
        public string CreaterName { get; set; }
        [MiicField(MiicStorageName = "CREATE_TIME", IsNotNull = true, MiicDbType = DbType.DateTime, Description = "创建时间")]
        public DateTime? CreateTime { get; set; }
        [MiicField(MiicStorageName = "LOGO_URL", IsNotNull = true, MiicDbType = DbType.String, Description = "LogoURL")]
        public string LogoUrl { get; set; }
        [MiicField(MiicStorageName = "SORT_NO", IsNotNull = true,IsIdentification=true, MiicDbType = DbType.Int32, Description = "排序")]
        public int? SortNo { get; set; }

    }
}
