using Miic.DB.SqlObject;
using Miic.DB.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Miic.Base;
using System.Data;
using Miic.Base.Setting;

namespace Miic.Friends.Community
{
    public class NoPersonKeywordView : Miic.Friends.Common.NoPersonKeywordView
    {
        /// <summary>
        /// 行业圈子ID
        /// </summary>
        public string CommunityID { get; set; }
        public NoPersonKeywordView() { }
        public MiicConditionCollections visitor(LabelInfoDao labelInfoDao) 
        {
            MiicConditionCollections result = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition communityIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<LabelInfo, string>(o => o.CommunityID),
                this.CommunityID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, communityIDCondition));
            MiicCondition keywordCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<LabelInfo, string>(o => o.LabelName),
                this.Keyword,
                DbType.String,
                MiicDBOperatorSetting.Like);
            result.Add(new MiicConditionLeaf(keywordCondition));
            MiicCondition validCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<LabelInfo, string>(o => o.Valid),
             ((int)MiicValidTypeSetting.Valid).ToString(),
              DbType.String,
              MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(validCondition));
            return result;
        }
    }
}
