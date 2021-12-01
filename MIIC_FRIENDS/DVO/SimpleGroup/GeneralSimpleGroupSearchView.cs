using Miic.DB.SqlObject;
using Miic.Friends.AddressBook;
using Miic.Friends.Community;
using Miic.Friends.Group;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Miic.DB.Setting;
using Miic.Base;
using System.Data;
using Miic.Base.Setting;
namespace Miic.Friends.General.SimpleGroup
{
    public abstract class GeneralSimpleGroupSearchView
    {
        /// <summary>
        /// 讨论组/圈子名/通讯录名
        /// </summary>
        protected string keyword;
        /// <summary>
        /// 讨论组/圈子名/通讯录名关键字
        /// </summary>
        public string Keyword
        {
            get { return this.keyword; }
            set { this.keyword = value; }
        }
        /// <summary>
        /// 讨论组ID
        /// </summary>
        public string GroupID { get; set; }

        /// <summary>
        /// 我的/某人的用户ID
        /// </summary>
        protected string userID;

        public virtual MiicConditionCollections visitor(GroupInfoDao groupInfo)
        {
            MiicConditionCollections result = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicConditionCollections keywordCondition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition groupNameCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.Name),
                keyword,
                DbType.String,
                MiicDBOperatorSetting.Like);
            keywordCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, groupNameCondition));
            MiicCondition groupRemarkCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.Remark),
              keyword,
              DbType.String,
              MiicDBOperatorSetting.Like);
            keywordCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.Or, groupRemarkCondition));
            result.Add(keywordCondition);
            MiicCondition userIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.MemberID),
                userID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(userIDCondition));
            MiicCondition validCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<GroupInfo, string>(o => o.Valid),
                ((int)MiicValidTypeSetting.Valid).ToString(),
                  DbType.String,
                  MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(validCondition));
            return result;
        }

        public virtual MiicConditionCollections visitor(AddressBookInfoDao addressBookInfo)
        {
            MiicConditionCollections result = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicConditionCollections keywordCondition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition addresserName = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserName),
                keyword,
                DbType.String,
                MiicDBOperatorSetting.Like);
            keywordCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, addresserName));
            MiicCondition remarkName = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<AddressBookInfo, string>(o => o.Remark),
                keyword,
                DbType.String,
                MiicDBOperatorSetting.Like);
            keywordCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.Or, remarkName));
            result.Add(keywordCondition);
            MiicCondition userIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.MyUserID),
                userID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(userIDCondition));
            return result;
        }
        public virtual MiicConditionCollections visitor(CommunityInfoDao communityInfo)
        {
            MiicConditionCollections result = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition keywordCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.Name),
              keyword,
              DbType.String,
              MiicDBOperatorSetting.Like);
            result.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, keywordCondition));
            MiicCondition userIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.MemberID),
                userID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(userIDCondition));
            MiicCondition validCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.Valid),
              ((int)MiicValidTypeSetting.Valid).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(validCondition));
            return result;
        }
      
    }
}
