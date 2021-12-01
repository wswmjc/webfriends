using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.AddressBook
{
    [MiicTable(MiicStorageName = "ADDRESS_BOOK_APPLI_INFO", Description = "通讯录申请审批表")]
    public partial class AddressBookApplicationInfo
    {
        [MiicField(MiicStorageName = "ID", IsNotNull = true, IsPrimaryKey = true, MiicDbType = DbType.String, Description = "唯一码")]
        public string ID { get; set; }
        [MiicField(MiicStorageName = "MY_USER_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "我的用户ID")]
        public string MyUserID { get; set; }
        [MiicField(MiicStorageName = "ADDRESSER_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "待加入通讯录人员ID")]
        public string AddresserID { get; set; }
        [MiicField(MiicStorageName = "ADDRESSER_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "待加入通讯录人员名称")]
        public string AddresserName { get; set; }
        [MiicField(MiicStorageName = "APPLICATION_TIME", IsNotNull = true, MiicDbType = DbType.DateTime, Description = "申请时间")]
        public DateTime? ApplicationTime { get; set; }
        [MiicField(MiicStorageName = "RESPONSE_TIME", MiicDbType = DbType.DateTime, Description = "响应审批时间")]
        public DateTime? ResponseTime { get; set; }
        [MiicField(MiicStorageName = "RESPONSE_STATUS", IsNotNull = true, MiicDbType = DbType.String, Description = "响应审批状态")]
        public string ResponseStatus { get; set; }
        [MiicField(MiicStorageName = "REMARK", MiicDbType = DbType.String, Description = "备注")]
        public string Remark { get; set; }
        [MiicField(MiicStorageName = "SORT_NO",IsIdentification=true, MiicDbType = DbType.Int32, Description = "排序")]
        public int? SortNo { get; set; }
    }
}
