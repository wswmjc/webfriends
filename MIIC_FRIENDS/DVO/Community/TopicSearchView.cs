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

namespace Miic.Friends.Community
{
    public class TopicSearchView
    {
        /// <summary>
        /// 查询者ID
        /// </summary>
        protected internal string userID;
        /// <summary>
        /// 行业圈子ID
        /// </summary>
        public string CommunityID { get; set; }
        /// <summary>
        /// 关键字
        /// </summary>
        public string Keyword { get; set; }
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
        public TopicSearchView() {
            Cookie cookie = new Cookie();
            string message = string.Empty;
            this.userID = cookie.GetCookie("SNS_ID", out message);
            if (string.IsNullOrEmpty(this.userID))
            {
                throw new Miic.MiicException.MiicCookieArgumentNullException("UserID不能为空，Cookie失效");
            }
        }
        public MiicConditionCollections visitor(MessageInfoDao messageInfoDao) 
        {
            MiicConditionCollections result = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition validCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<TopicInfo, string>(o => o.Valid),
                ((int)MiicValidTypeSetting.Valid).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, validCondition));
            MiicCondition keywordCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<TopicInfo, string>(o => o.TopicContent),
                this.Keyword,
                DbType.String,
                MiicDBOperatorSetting.Like);
            result.Add(new MiicConditionLeaf(keywordCondition));
            MiicCondition communityIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<TopicInfo, string>(o => o.CommunityID),
                this.CommunityID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(communityIDCondition));
            
            return result;
        }
    }
}
