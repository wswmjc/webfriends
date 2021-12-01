using Miic.Base;
using Miic.Base.Setting;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Moments;
using Miic.Friends.Moments.Behavior;
using Miic.MiicException;
using System.Data;

namespace Miic.Friends.Moments
{
    public class MyBehaviorView
    {
        /// <summary>
        /// 朋友圈信息发布ID
        /// </summary>
        public string PublishID { get; set; }
        /// <summary>
        /// 我的用户ID
        /// </summary>
        public string UserID { get; private set; }

        public MyBehaviorView()
        {
            string message = string.Empty;
            Cookie cookie = new Cookie();
            UserID = cookie.GetCookie("SNS_ID", out message);
            if (string.IsNullOrEmpty(UserID))
            {
                throw new MiicCookieArgumentNullException("UserID不能为空，Cookie失效");
            }
        }

        /// <summary>
        /// 收藏DAO访问器
        /// </summary>
        /// <param name="collectInfoDao">收藏</param>
        /// <returns></returns>
        public MiicConditionCollections visitor(Moments.Behavior.CollectInfoDao collectInfoDao)
        {
            MiicConditionCollections condition = new MiicConditionCollections();
            MiicCondition publishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.PublishID),
               PublishID,
               DbType.String,
               MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, publishIDCondition));
            MiicCondition collectorIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.CollectorID),
                UserID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(collectorIDCondition));
            MiicCondition validCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.CollectValid),
                ((int)MiicValidTypeSetting.Valid).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(validCondition));
            return condition;
        }

        /// <summary>
        /// 点赞DAO访问器
        /// </summary>
        /// <param name="praiseInfoDao">点赞</param>
        /// <returns></returns>
        public MiicConditionCollections visitor(Moments.Behavior.PraiseInfoDao praiseInfoDao)
        {
            MiicConditionCollections condition = new MiicConditionCollections();
            MiicCondition publishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PraiseInfo, string>(o => o.PublishID),
               PublishID,
               DbType.String,
               MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, publishIDCondition));
            MiicCondition praiserIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PraiseInfo, string>(o => o.PraiserID),
                UserID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(praiserIDCondition));
            return condition;
        }

        /// <summary>
        /// 举报DAO访问器
        /// </summary>
        /// <param name="reportInfoDao">举报</param>
        /// <returns></returns>
        public MiicConditionCollections visitor(Moments.Behavior.ReportInfoDao reportInfoDao)
        {
            MiicConditionCollections condition = new MiicConditionCollections();
            MiicCondition publishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfo, string>(o => o.PublishID),
               PublishID,
               DbType.String,
               MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, publishIDCondition));
            MiicCondition reporterIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfo, string>(o => o.ReporterID),
                UserID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(reporterIDCondition));
            return condition;
        }

        /// <summary>
        /// 点踩DAO访问器
        /// </summary>
        /// <param name="treadInfoDao">点踩</param>
        /// <returns></returns>
        public MiicConditionCollections visitor(Moments.Behavior.TreadInfoDao treadInfoDao)
        {
            MiicConditionCollections condition = new MiicConditionCollections();
            MiicCondition publishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<TreadInfo, string>(o => o.PublishID),
               PublishID,
               DbType.String,
               MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, publishIDCondition));
            MiicCondition treaderIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<TreadInfo, string>(o => o.TreaderID),
                UserID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(treaderIDCondition));
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
            querySql = "SELECT * FROM dbo.GetMyMomentsBehaviorFlags('" + UserID + "','" + PublishID + "')";
            return querySql;
        }
    }
}
