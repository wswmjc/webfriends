using Miic.Base;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Community.Setting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Community
{
    public  class ApproveView
    {
        /// <summary>
        /// 申请者/受邀者
        /// </summary>
        public string MemberID { get; set; }
        public string MemberName { get; set; }
        /// <summary>
        /// 行业圈子ID
        /// </summary>
        public string CommunityID { get; set; }
        public ApproveView() { }
        public MiicConditionCollections visitor(CommunityInfoDao communityInfoDao)
        {
            MiicConditionCollections result = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition applicantIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityApplicationInfo, string>(o => o.MemberID),
                MemberID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, applicantIDCondition));
            MiicConditionCollections responseStatusCondition = new MiicConditionCollections();
            MiicCondition applyCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityApplicationInfo, string>(o => o.ResponseStatus),
                ((int)ApplyStatusSetting.Apply).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            responseStatusCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No,applyCondition));
            MiicCondition inviteCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityApplicationInfo, string>(o => o.ResponseStatus),
                ((int)ApplyStatusSetting.Invite).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            responseStatusCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.Or,inviteCondition));
            result.Add(responseStatusCondition);
            MiicCondition comminityIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityApplicationInfo, string>(o => o.CommunityID),
                CommunityID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(comminityIDCondition));
            return result;
        }
    }
}
