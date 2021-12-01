using Miic.Base;
using Miic.Base.Setting;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.AddressBook;
using Miic.Friends.Community;
using Miic.Friends.Moments;
using Miic.Friends.User;
using System;
using System.Collections.Generic;
using System.Data;

namespace Miic.Friends.Common
{
    public abstract class KeywordView
    {
        /// <summary>
        /// 查询者ID
        /// </summary>
        protected internal string userID;
        /// <summary>
        /// 关键字
        /// </summary>
        public string Keyword { get; set; }

        public KeywordView()
        {

        }

        public MiicConditionCollections visitor(UserInfoDao userInfoDao)
        {
            MiicConditionCollections result = new MiicConditionCollections();
            //关键字
            MiicConditionCollections keywordCondition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition socialCode = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.User.SimpleUserView, string>(o => o.SocialCode),
                this.Keyword,
                DbType.String,
                MiicDBOperatorSetting.Like);
            keywordCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No,socialCode ));
            MiicCondition userName = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.User.SimpleUserView, string>(o => o.UserName),
                this.Keyword,
                DbType.String,
                MiicDBOperatorSetting.Like);
            keywordCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.Or, userName));
            result.Add(keywordCondition);
          
            return result;
        }
    }
}
