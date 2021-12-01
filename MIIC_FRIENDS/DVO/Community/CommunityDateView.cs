using Miic.Base;
using Miic.Base.Setting;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Common;
using Miic.MiicException;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Community
{
    public class CommunityDateView 
    {
        /// <summary>
        /// 查询者ID
        /// </summary>
        protected internal string userID;
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
        /// <summary>
        /// 年份
        /// </summary>
        public string Year { get; set; }
        /// <summary>
        /// 月份
        /// </summary>
        public string Month { get; set; }
        /// <summary>
        /// 行业圈子ID
        /// </summary>
        public string CommunityID { get; set; }
        /// <summary>
        /// 标签ID
        /// </summary>
        public string LabelID { get; set; }

        public CommunityDateView() {
            Cookie cookie = new Cookie();
            string message = string.Empty;
            this.userID = cookie.GetCookie("SNS_ID", out message);
            if (string.IsNullOrEmpty(this.userID))
            {
                throw new MiicCookieArgumentNullException("UserID不能为空，Cookie失效");
            }
        }

        public MiicConditionCollections vistor(PublishInfoDao publishDao)
        {
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition communityIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.CommunityID),
                CommunityID,
                 DbType.String,
                  MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, communityIDCondition));
            MiicCondition labelIDIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelID),
                LabelID,
                 DbType.String,
                  MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(labelIDIDCondition));
            MiicCondition validCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.Valid),
                ((int)MiicValidTypeSetting.Valid).ToString(),
                 DbType.String,
                  MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(validCondition));
            MiicCondition editStatusCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.EditStatus),
                                                      ((int)MiicYesNoSetting.No).ToString(),
                                                      DbType.String,
                                                      MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(editStatusCondition));
            MiicConditionCollections dateCondition = new MiicConditionCollections();
            MiicCondition yearCondition = new MiicCondition(MiicSimpleDateTimeFunction.YearFunc<PublishInfo, DateTime?>(o => o.PublishTime),
                                                        Year,
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Equal);
            dateCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, yearCondition));
            if (!string.IsNullOrEmpty(Month))
            {
                MiicCondition monthCondition = new MiicCondition(MiicSimpleDateTimeFunction.MonthFunc<PublishInfo, DateTime?>(o => o.PublishTime),
                                                            Month,
                                                            DbType.String,
                                                            MiicDBOperatorSetting.Equal);
                dateCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.And, monthCondition));
            }
            condition.Add(dateCondition);
            return condition;
        }
    }
}
