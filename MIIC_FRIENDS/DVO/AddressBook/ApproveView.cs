using Miic.Base;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.AddressBook.Setting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Miic.Friends.AddressBook
{
    public class ApproveView
    {
        public string MyUserID { get; private set; }
        public string MyUserName { get; private set; }
        public string ApplicantID { get; set; }
        public string ApplicantName { get; set; }
        public DateTime? ApplicationTime { get; set; }
        public ApproveView() 
        {
            string message = string.Empty;
            Cookie cookie = new Cookie();
            this.MyUserID = cookie.GetCookie("SNS_ID", out message);
            this.MyUserName = HttpUtility.UrlDecode(cookie.GetCookie("SNS_UserName", out message));
        }
        public MiicConditionCollections visitor(AddressBookInfoDao addressBookInfoDao) 
        {
            MiicConditionCollections result = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition myUserIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo, string>(o => o.AddresserID),
                MyUserID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, myUserIDCondition));
            MiicCondition applicantIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo, string>(o => o.MyUserID),
                ApplicantID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(applicantIDCondition));
            MiicCondition responseStatusCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo, string>(o => o.ResponseStatus),
                ((int)ApplyStatusSetting.Apply).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(responseStatusCondition));
            return result;
        }
    }
}
