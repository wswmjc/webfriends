using Miic.Base;
using Miic.Base.Setting;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Community;
using Miic.Friends.Community.Behavior;
using Miic.MiicException;
using System.Data;

namespace Miic.Friends.Community
{
    public class MyCommunityBehaviorView
    {
        public string PublishID { get; set; }
        public string LoginUserID { get; private set; }

        public MyCommunityBehaviorView()
        {
            string message = string.Empty;
            Cookie cookie = new Cookie();
            LoginUserID = cookie.GetCookie("SNS_ID", out message);
            if (string.IsNullOrEmpty(LoginUserID))
            {
                throw new MiicCookieArgumentNullException("LoginUserID不能为空，Cookie失效");
            }
        }

        /// <summary>
        /// 收藏DAO访问器
        /// </summary>
        /// <param name="communityCollectInfoDao">收藏</param>
        /// <returns></returns>
        public MiicConditionCollections visitor(Community.Behavior.CollectInfoDao communityCollectInfoDao)
        {
            MiicConditionCollections condition = new MiicConditionCollections();
            MiicCondition publishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.PublishID),
               PublishID,
               DbType.String,
               MiicDBOperatorSetting.Equal);
            MiicCondition collectorIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.CollectorID),
                LoginUserID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            MiicCondition validCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.CollectValid),
                ((int)MiicValidTypeSetting.Valid).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, publishIDCondition));
            condition.Add(new MiicConditionLeaf(collectorIDCondition));
            condition.Add(new MiicConditionLeaf(validCondition));
            return condition;
        }

        /// <summary>
        /// 点赞DAO访问器
        /// </summary>
        /// <param name="communityPraiseInfoDao">点赞</param>
        /// <returns></returns>
        public MiicConditionCollections visitor(Community.Behavior.PraiseInfoDao communityPraiseInfoDao)
        {
            MiicConditionCollections condition = new MiicConditionCollections();
            MiicCondition publishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PraiseInfo, string>(o => o.PublishID),
               PublishID,
               DbType.String,
               MiicDBOperatorSetting.Equal);
            MiicCondition collectorIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PraiseInfo, string>(o => o.PraiserID),
                LoginUserID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, publishIDCondition));
            condition.Add(new MiicConditionLeaf(collectorIDCondition));
            return condition;
        }

        /// <summary>
        /// 举报DAO访问器
        /// </summary>
        /// <param name="communityReportInfoDao">举报</param>
        /// <returns></returns>
        public MiicConditionCollections visitor(Community.Behavior.ReportInfoDao communityReportInfoDao)
        {
            MiicConditionCollections condition = new MiicConditionCollections();
            MiicCondition publishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfo, string>(o => o.PublishID),
               PublishID,
               DbType.String,
               MiicDBOperatorSetting.Equal);
            MiicCondition collectorIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfo, string>(o => o.ReporterID),
                LoginUserID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, publishIDCondition));
            condition.Add(new MiicConditionLeaf(collectorIDCondition));
            return condition;
        }

        /// <summary>
        /// 点踩DAO访问器
        /// </summary>
        /// <param name="communityTreadInfoDao">点踩</param>
        /// <returns></returns>
        public MiicConditionCollections visitor(Community.Behavior.TreadInfoDao communityTreadInfoDao)
        {
            MiicConditionCollections condition = new MiicConditionCollections();
            MiicCondition publishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<TreadInfo, string>(o => o.PublishID),
               PublishID,
               DbType.String,
               MiicDBOperatorSetting.Equal);
            MiicCondition collectorIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<TreadInfo, string>(o => o.TreaderID),
                LoginUserID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, publishIDCondition));
            condition.Add(new MiicConditionLeaf(collectorIDCondition));
            return condition;
        }

        /// <summary>
        /// 获取登录用户的行为FLAG 查询语句
        /// </summary>
        /// <param name="PublishInfoDao"></param>
        /// <returns>querySql</returns>
        public string visitor(PublishInfoDao PublishInfoDao)
        {
            string querySql = string.Empty;
            querySql = "SELECT * FROM dbo.GetMyCommunityBehaviorFlags('" + LoginUserID + "','" + PublishID + "')";
            return querySql;
        }
    }
}
