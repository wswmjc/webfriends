using Miic.Base;
using Miic.Base.Setting;
using Miic.Friends.Common.Setting;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.MiicException;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Notice
{
   public  class MyNoticeView
    {
       /// <summary>
       /// 我的用户ID
       /// </summary>
       public string MyUserID { get; private set; }
       /// <summary>
       /// 业务类别（通知类型）
       /// </summary>
       public BusinessTypeSetting BusinessType { get; set; }
       public MyNoticeView() 
       {
           Cookie cookie = new Cookie();
           string message = string.Empty;
           this.MyUserID = cookie.GetCookie("SNS_ID", out message);
           if (string.IsNullOrEmpty(this.MyUserID))
           {
               throw new MiicCookieArgumentNullException("UserID不能为空，Cookie失效");
           }
       }
       public MiicConditionCollections visitor(NoticeInfoDao noticeInfoDao) 
       {
           MiicConditionCollections result = new MiicConditionCollections(MiicDBLogicSetting.No);
           MiicCondition noticerCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, string>(o => o.NoticerID),
               this.MyUserID,
               DbType.String,
               MiicDBOperatorSetting.Equal);
           result.Add(new MiicConditionLeaf(MiicDBLogicSetting.No,noticerCondition));
           MiicCondition businessTypeCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, string>(o => o.Source),
               ((int)BusinessType).ToString(),
               DbType.String,
               MiicDBOperatorSetting.Equal);
           result.Add(new MiicConditionLeaf(businessTypeCondition));
           MiicCondition readStatusCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, string>(o => o.ReadStatus),
               ((int)MiicReadStatusSetting.UnRead).ToString(),
               DbType.String,
               MiicDBOperatorSetting.Equal);
           result.Add(new MiicConditionLeaf(readStatusCondition));
           return result;
       }
       public MiicConditionCollections visitor(MessageInfoDao messageInfoDao) 
       {
           throw new NotImplementedException();
       }
    }
}
