using Miic.Base;
using Miic.Base.Setting;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Group;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Group
{
    public class TopicSearchView
    {
        /// <summary>
        /// 查询者ID
        /// </summary>
        protected internal string userID;
        /// <summary>
        /// 讨论组ID
        /// </summary>
        public string GroupID { get; set; }
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
        public TopicSearchView()
        {
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
            MiicCondition groupIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<TopicInfo, string>(o => o.GroupID),
                this.GroupID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(groupIDCondition));

            return result;
        }
    }
}
