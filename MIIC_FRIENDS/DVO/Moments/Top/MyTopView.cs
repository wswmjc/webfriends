using Miic.Base;
using Miic.Base.Setting;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Common.Setting;
using Miic.MiicException;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Moments
{
    public class MyTopView:GeneralTopView
    {
        public string UserID
        {
            get
            {
                return this.userID;
            }
        }

        public MyTopView()
        {
            Cookie cookie = new Cookie();
            string message = string.Empty;
            this.userID = cookie.GetCookie("SNS_ID", out message);
            if (string.IsNullOrEmpty(this.UserID))
            {
                throw new MiicCookieArgumentNullException("UserID不能为空，Cookie失效");
            }
        }

        public override MiicConditionCollections visitor(PublishInfoDao publishInfoDao)
        {
            MiicConditionCollections condition = new MiicConditionCollections();

            MiicCondition editStatusCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.EditStatus),
                                                        ((int)MiicYesNoSetting.No).ToString(),
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No,editStatusCondition));

            MiicConditionCollections userIDCondition = new MiicConditionCollections();
            //发表人是用户
            MiicCondition createrIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.CreaterID),
                                                       userID,
                                                       DbType.String,
                                                       MiicDBOperatorSetting.Equal);
            //发表人是通讯录好友
            MiicCondition myUserIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.MyUserID),
                                                      userID,
                                                      DbType.String,
                                                      MiicDBOperatorSetting.Equal);

            if (Belong == PublishInfoBelongSetting.Main)
            {

                userIDCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, createrIDCondition));
                userIDCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.Or, myUserIDCondition));
            }
            else if (Belong == PublishInfoBelongSetting.Self)
            {
                userIDCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, createrIDCondition));
            }
            else if (Belong == PublishInfoBelongSetting.Other)
            {
                throw new ArgumentOutOfRangeException("个人查询top，不能传入他人页面");
            }

            condition.Add(userIDCondition);

            if (Belong == PublishInfoBelongSetting.Main)
            {
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
            }

            //默认时间倒序排列
            List<MiicOrderBy> order = new List<MiicOrderBy>();
            order.Add(new MiicOrderBy()
            {
                Desc = false,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, DateTime?>(o => o.PublishTime)
            });
            condition.order = order;

            return condition;
        }
    }
}
