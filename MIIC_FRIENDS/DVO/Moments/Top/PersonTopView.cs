using Miic.Base;
using Miic.Base.Setting;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Moments
{
    public class PersonTopView:GeneralTopView
    {
        public string UserID
        {
            get { return this.userID; }
            set { this.userID = value; }
        }

        public override MiicConditionCollections visitor(PublishInfoDao publishInfoDao)
        {
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);

            MiicCondition editStatusCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.EditStatus),
                                                        ((int)MiicYesNoSetting.No).ToString(),
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No,editStatusCondition));

            //发表人是用户
            MiicCondition createrIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.CreaterID),
                                                       this.UserID,
                                                       DbType.String,
                                                       MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(createrIDCondition));

            //默认时间倒序排列
            List<MiicOrderBy> order = new List<MiicOrderBy>();
            order.Add(new MiicOrderBy()
            {
                Desc = false,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.PublishTime)
            });
            condition.order = order;

            return condition;
        }
    }
}
