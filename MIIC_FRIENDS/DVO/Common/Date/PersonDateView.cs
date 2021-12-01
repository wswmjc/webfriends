using Miic.Base;
using Miic.Base.Setting;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Moments;
using System;
using System.Data;

namespace Miic.Friends.Common
{
    public class PersonDateView:GeneralDateView
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserID
        {
            get { return this.userID; }
            set { this.userID = value; }
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

            //发表人是用户
            MiicCondition createrIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.CreaterID),
                                                       this.UserID,
                                                       DbType.String,
                                                       MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(createrIDCondition));

            return condition;
        }
    }
}
