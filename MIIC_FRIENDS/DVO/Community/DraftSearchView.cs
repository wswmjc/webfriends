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

namespace Miic.Friends.Community
{
    public class DraftSearchView
    {
        /// <summary>
        /// 查询者ID
        /// </summary>
        protected internal string userID;
        /// <summary>
        /// 发表人ID
        /// </summary>
        public string UserID
        {
            get
            {
                return this.userID;
            }
        }
        /// <summary>
        /// 关键字
        /// </summary>
        public string Keyword { get; set; }

        public DraftSearchView()
        {
            Cookie cookie = new Cookie();
            string message = string.Empty;
            this.userID = cookie.GetCookie("SNS_ID", out message);
            if (string.IsNullOrEmpty(this.UserID))
            {
                throw new MiicCookieArgumentNullException("UserID不能为空，Cookie失效");
            }
        }
        /// <summary>
        /// 朋友圈DAO访问器
        /// </summary>
        /// <param name="publishInfoDao"></param>
        /// <returns></returns>
        public MiicConditionCollections visitor(PublishInfoDao publishInfoDao)
        {
            MiicConditionCollections condition = new MiicConditionCollections();
            //编辑状态为草稿
            MiicCondition editStatusCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.EditStatus),
                                                        ((int)MiicYesNoSetting.Yes).ToString(),
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, editStatusCondition));

            MiicConditionCollections keyworCondition = new MiicConditionCollections();
            MiicCondition microTitleCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Title),
                                                        Keyword,
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Like);
            keyworCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, microTitleCondition));
            MiicCondition microContentCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Content),
                                                        Keyword,
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Like);
            keyworCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.Or, microContentCondition));
            condition.Add(keyworCondition);

            MiicCondition createrIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.CreaterID),
                                                       userID,
                                                       DbType.String,
                                                       MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(createrIDCondition));

            MiicCondition microTypeCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.PublishType),
                                                       ((int)PublishInfoTypeSetting.Long).ToString(),
                                                       DbType.String,
                                                       MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(microTypeCondition));

            return condition;
        }
    }
}
