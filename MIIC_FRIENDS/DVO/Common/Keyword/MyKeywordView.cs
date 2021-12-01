using Miic.Base;
using Miic.Base.Setting;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Moments;
using Miic.MiicException;
using System.Data;

namespace Miic.Friends.Common
{
    public class MyKeywordView : KeywordView
    {

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
        public MyKeywordView()
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
        /// 朋友圈浏览DAO访问器
        /// </summary>
        /// <param name="momentsBrowseInfoDao"></param>
        /// <returns></returns>
        public MiicConditionCollections visitor(Moments.Behavior.BrowseInfoDao momentsBrowseInfoDao)
        {
            MiicConditionCollections condition = new MiicConditionCollections();
            MiicCondition isHintCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.BrowseInfo, string>(o => o.IsHinted),
                                                        ((int)MiicYesNoSetting.Yes).ToString(),
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, isHintCondition));
            MiicConditionCollections keyworCondition = new MiicConditionCollections();
            MiicCondition momentsTitleCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.PublishInfo, string>(o => o.Title),
                                                        Keyword,
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Like);
            keyworCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, momentsTitleCondition));
            MiicCondition momentsContentCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.PublishInfo, string>(o => o.Content),
                                                        Keyword,
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Like);
            keyworCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.Or, momentsContentCondition));
            condition.Add(keyworCondition);
            MiicCondition browserIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.BrowseInfo, string>(o => o.BrowserID),
                                                        UserID,
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(browserIDCondition));
            return condition;
        }

        /// <summary>
        /// 行业圈子浏览DAO访问器
        /// </summary>
        /// <param name="communityBrowseInfoDao"></param>
        /// <returns></returns>
        public MiicConditionCollections visitor(Community.Behavior.BrowseInfoDao communityBrowseInfoDao)
        {
            MiicConditionCollections condition = new MiicConditionCollections();
            MiicCondition isHintCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Community.Behavior.BrowseInfo, string>(o => o.IsHinted),
                                                        ((int)MiicYesNoSetting.Yes).ToString(),
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, isHintCondition));
            MiicConditionCollections keyworCondition = new MiicConditionCollections();
            MiicCondition momentsTitleCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Community.PublishInfo, string>(o => o.Title),
                                                        Keyword,
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Like);
            keyworCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, momentsTitleCondition));
            MiicCondition momentsContentCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Community.PublishInfo, string>(o => o.Content),
                                                        Keyword,
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Like);
            keyworCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.Or, momentsContentCondition));
            condition.Add(keyworCondition);
            MiicCondition browserIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Community.Behavior.BrowseInfo, string>(o => o.BrowserID),
                                                        UserID,
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(browserIDCondition));
            return condition;
        }


        /// <summary>
        /// 朋友圈收藏DAO访问器
        /// </summary>
        /// <param name="momentsCollectInfoDao"></param>
        /// <returns></returns>
        public MiicConditionCollections visitor(Moments.Behavior.CollectInfoDao momentsCollectInfoDao)
        {
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);

            //收藏人
            MiicCondition collecterCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.CollectInfo, string>(o => o.CollectorID),
                                                        UserID,
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, collecterCondition));

            //标题或内容匹配
            MiicConditionCollections keyworCondition = new MiicConditionCollections();
            MiicCondition titleCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<Moments.PublishInfo, string>(o => o.Title),
                                                        Keyword,
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Like);
            keyworCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, titleCondition));
            MiicCondition contentCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<Moments.PublishInfo, string>(o => o.Content),
                                                        Keyword,
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Like);
            keyworCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.Or, contentCondition));
            condition.Add(keyworCondition);

            //有效收藏
            MiicCondition collectValidCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.CollectInfo, string>(o => o.CollectValid),
                                                       ((int)MiicValidTypeSetting.Valid).ToString(),
                                                       DbType.String,
                                                       MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(collectValidCondition));

            return condition;
        }

        /// <summary>
        /// 行业圈子收藏DAO访问器
        /// </summary>
        /// <param name="communityCollectInfoDao"></param>
        /// <returns></returns>
        public MiicConditionCollections visitor(Community.Behavior.CollectInfoDao communityCollectInfoDao)
        {
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);
            //收藏人
            MiicCondition collecterCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Community.Behavior.CollectInfo, string>(o => o.CollectorID),
                                                        UserID,
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, collecterCondition));
            //标题或内容匹配
            MiicConditionCollections keyworCondition = new MiicConditionCollections();
            MiicCondition titleCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<Community.PublishInfo, string>(o => o.Title),
                                                        Keyword,
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Like);
            keyworCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, titleCondition));
            MiicCondition contentCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<Community.PublishInfo, string>(o => o.Content),
                                                        Keyword,
                                                        DbType.String,
                                                        MiicDBOperatorSetting.Like);
            keyworCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.Or, contentCondition));
            condition.Add(keyworCondition);

            //有效收藏
            MiicCondition collectValidCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Community.Behavior.CollectInfo, string>(o => o.CollectValid),
                                                       ((int)MiicValidTypeSetting.Valid).ToString(),
                                                       DbType.String,
                                                       MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(collectValidCondition));

            return condition;
        }
    }
}
