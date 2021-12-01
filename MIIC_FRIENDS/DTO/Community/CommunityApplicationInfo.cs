using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Community
{
    [MiicTable(MiicStorageName = "COMMUNITY_APPLI_INFO", Description = "加入圈子申请表")]
    public partial class CommunityApplicationInfo
    {
        [MiicField(MiicStorageName = "ID", IsNotNull = true, IsPrimaryKey = true, MiicDbType = DbType.String, Description = "唯一码")]
        public string ID { get; set; }
        [MiicField(MiicStorageName = "COMMUNITY_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "发布ID")]
        public string CommunityID { get; set; }
        [MiicField(MiicStorageName = "MEMBER_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "申请者ID")]
        public string MemberID { get; set; }
        [MiicField(MiicStorageName = "MEMBER_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "申请者名称")]
        public string MemberName { get; set; }
        [MiicField(MiicStorageName = "APPLICATION_TIME", IsNotNull = true, MiicDbType = DbType.DateTime, Description = "申请时间")]
        public DateTime? ApplicationTime { get; set; }
        [MiicField(MiicStorageName = "RESPONSE_TIME", MiicDbType = DbType.DateTime, Description = "响应时间")]
        public DateTime? ResponseTime { get; set; }
        [MiicField(MiicStorageName = "RESPONSE_STATUS", IsNotNull = true, MiicDbType = DbType.String, Description = "响应状态")]
        public string ResponseStatus { get; set; }
        [MiicField(MiicStorageName = "REMARK", MiicDbType = DbType.String, Description = "备注")]
        public string Remark { get; set; }
        [MiicField(MiicStorageName = "SORT_NO", IsNotNull = true, IsIdentification = true, MiicDbType = DbType.Int32, Description = "排序")]
        public int? SortNo { get; set; }
    }
}
