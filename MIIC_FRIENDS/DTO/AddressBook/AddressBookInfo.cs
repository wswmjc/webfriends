using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.AddressBook
{
    [MiicTable(MiicStorageName = "ADDRESS_BOOK_INFO", Description = "通讯录表")]
    public partial class AddressBookInfo
    {
        [MiicField(MiicStorageName = "ID", IsNotNull = true, IsPrimaryKey = true, MiicDbType = DbType.String, Description = "唯一码")]
        public string ID { get; set; }
        [MiicField(MiicStorageName = "MY_USER_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "我的用户ID")]
        public string MyUserID { get; set; }
        [MiicField(MiicStorageName = "ADDRESSER_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "通讯录用户ID")]
        public string AddresserID { get; set; }
        [MiicField(MiicStorageName = "ADDRESSER_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "通讯录用户名称")]
        public string AddresserName { get; set; }
        [MiicField(MiicStorageName = "IS_BLACK_LIST", IsNotNull = true, MiicDbType = DbType.String, Description = "是否加入黑名单")]
        public string IsBlackList { get; set; }
        [MiicField(MiicStorageName = "CAN_SEE_ME", IsNotNull = true, MiicDbType = DbType.String, Description = "通讯录人员是否可以看我")]
        public string CanSeeMe { get; set; }
        [MiicField(MiicStorageName = "CAN_SEE_ME_TIME", IsNotNull = true, MiicDbType = DbType.DateTime, Description = "设置是否可以看我时间")]
        public DateTime? CanSeeMeTime { get; set; }
        [MiicField(MiicStorageName = "CAN_SEE_ADDRESSER", IsNotNull = true, MiicDbType = DbType.String, Description = "是否可以看通讯录人员的信息")]
        public string CanSeeAddresser { get; set; }
        [MiicField(MiicStorageName = "CAN_SEE_ADDRESSER_TIME", IsNotNull = true, MiicDbType = DbType.DateTime, Description = "设置是否可以看通讯录人员信息的时间")]
        public DateTime? CanSeeAddresserTime { get; set; }
        [MiicField(MiicStorageName = "APPLICATION_TIME", MiicDbType = DbType.DateTime, Description = "申请时间")]
        public DateTime? ApplicationTime { get; set; }
        [MiicField(MiicStorageName = "JOIN_TIME", IsNotNull = true, MiicDbType = DbType.DateTime, Description = "同意加入时间")]
        public DateTime? JoinTime { get; set; }
         [MiicField(MiicStorageName = "OFTEN_USED", IsNotNull = true, MiicDbType = DbType.String, Description = "是否经常使用")]
        public string OftenUsed { get; set; }
        [MiicField(MiicStorageName = "REMARK", MiicDbType = DbType.String, Description = "备注（昵称）")]
        public string Remark { get; set; }
       
    }
}
