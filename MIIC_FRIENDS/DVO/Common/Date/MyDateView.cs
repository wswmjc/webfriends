using Miic.Base;
using Miic.Base.Setting;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Moments;
using Miic.MiicException;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Common
{
    public class MyDateView:GeneralDateView
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserID
        {
            get
            {
                return this.userID;
            }
        }
        public MyDateView() 
        {
            Cookie cookie = new Cookie();
            string message = string.Empty;
            this.userID = cookie.GetCookie("SNS_ID", out message);
            if (string.IsNullOrEmpty(this.userID))
            {
                throw new MiicCookieArgumentNullException("UserID不能为空，Cookie失效");
            }
        }
        public override MiicConditionCollections visitor(PublishInfoDao publishInfoDao)
        {
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);

            MiicCondition editStatusCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.EditStatus),
                                                        ((int)MiicYesNoSetting.No).ToString(),
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No,editStatusCondition));

            MiicConditionCollections dateCondition = new MiicConditionCollections();
            MiicCondition yearCondition = new MiicCondition(MiicSimpleDateTimeFunction.YearFunc<AddressPublishInfo, DateTime?>(o => o.PublishTime),
                                                        Year,
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Equal);
            dateCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, yearCondition));
            if (!string.IsNullOrEmpty(Month))
            {
                MiicCondition monthCondition = new MiicCondition(MiicSimpleDateTimeFunction.MonthFunc<AddressPublishInfo, DateTime?>(o => o.PublishTime),
                                                            Month,
                                                            DbType.String,
                                                            MiicDBOperatorSetting.Equal);
                dateCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.And, monthCondition));
            }
            condition.Add(dateCondition);

            MiicConditionCollections userIDCondition = new MiicConditionCollections();
            //发表人是用户
            MiicCondition createrIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.CreaterID),
                                                       this.UserID,
                                                       DbType.String,
                                                       MiicDBOperatorSetting.Equal);
            userIDCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, createrIDCondition));
            //发表人是通讯录好友
            MiicCondition myUserIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.MyUserID),
                                                      this.UserID,
                                                      DbType.String,
                                                      MiicDBOperatorSetting.Equal);
            userIDCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.Or, myUserIDCondition));
            condition.Add(userIDCondition);

            //通讯录的好友对自己可见
            MiicConditionCollections seeCondition = new MiicConditionCollections();
            //如果是自己发表且没有好友
            MiicCondition selfNullSeeCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.CanSeeAddresser),
                                                       null,
                                                       DbType.String,
                                                       MiicDBOperatorSetting.IsNull);
            seeCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, selfNullSeeCondition));
            //有好友，那么好友对自己可见
            MiicCondition canSeeAddresserCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.CanSeeAddresser),
                                                        ((int)MiicYesNoSetting.Yes).ToString(),
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Equal);
            seeCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.Or, canSeeAddresserCondition));
            condition.Add(seeCondition);
            return condition;
        }
    }
}
