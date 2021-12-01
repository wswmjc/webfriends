using Miic.Base;
using Miic.Base.Setting;
using Miic.Common;
using Miic.Friends.Common.Setting;
using Miic.Friends.Common;
using Miic.DB;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Notice;
using Miic.Log;
using Miic.MiicException;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Miic.Friends.Moments.Behavior;
using Miic.Friends.Community;
using Miic.Manage.User;
using Miic.Manage.User.Setting;

namespace Miic.Friends.Moments
{
    public partial class PublishInfoDao : RelationCommon<PublishInfo, AccessoryInfo>, IPublishInfo
    {

        private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
        private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;

        public PublishInfoDao() { }

        /// <summary>
        /// 将临时存储路径替换为指定朋友圈的路径
        /// </summary>
        /// <param name="path">临时存储路径</param>
        /// <returns>指定朋友圈的存储路径</returns>
        private string GetRealSavePath(string path)
        {
            string savePath = string.Empty;
            savePath = path.Replace("PublishInfoAcc", "Moments");
            return savePath;
        }

        bool ICommon<PublishInfo>.Insert(PublishInfo publishInfo)
        {
            Contract.Requires<ArgumentNullException>(publishInfo != null, "参数publishInfo：不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(publishInfo.ID), "参数publishInfo.ID：不能为空！");
            bool result = false;
            string message = string.Empty;
            List<string> sqls = new List<string>();
            /*------------------------------朋友圈初始化值-----------------------------------*/
            //浏览总数
            publishInfo.BrowseNum = 0;
            //点赞数
            publishInfo.PraiseNum = 0;
            //踩数
            publishInfo.TreadNum = 0;
            //转发数
            publishInfo.TransmitNum = 0;
            //被评论数
            publishInfo.CommentNum = 0;
            //举报数
            publishInfo.ReportNum = 0;
            //收藏总数
            publishInfo.CollectNum = 0;
            //微博是否有附件
            publishInfo.HasAcc = ((int)MiicYesNoSetting.No).ToString();
            /*------------------------------用户初始化值-----------------------------------*/
            sqls.Add(DBService.InsertSql<PublishInfo>(publishInfo, out message));

            //积分
            if (publishInfo.EditStatus == ((int)MiicYesNoSetting.No).ToString())
            {
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = publishInfo.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Publish).ToString(),
                    Score = ScoreConfig.Score.PublishScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = publishInfo.CreaterID,
                    UserName = publishInfo.CreaterName
                }, out message));
            }
            try
            {
                result = dbService.excuteSqls(sqls, out message);
            }
            catch (Exception ex)
            {
                Config.IlogicLogService.Write(new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                });
            }
            if (result == true)
            {
                InsertCache(publishInfo);
            }
            return result;
        }

        bool ICommon<PublishInfo>.Update(PublishInfo publishInfo)
        {
            Contract.Requires<ArgumentNullException>(publishInfo != null, "参数publishInfo:不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(publishInfo.ID), "参数publishInfo.ID:不能为空，因为是主键");
            string message = string.Empty;
            bool result = false;
            List<string> sqls = new List<string>();
            PublishInfo temp = ((ICommon<PublishInfo>)this).GetInformation(publishInfo.ID);
            if (temp.EditStatus == ((int)MiicYesNoSetting.No).ToString() && publishInfo.EditStatus == ((int)MiicYesNoSetting.Yes).ToString())
            {
                //积分
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = publishInfo.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Publish).ToString(),
                    Score = -1 * ScoreConfig.Score.PublishScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = temp.CreaterID,
                    UserName = temp.CreaterName
                }, out message));
            }

            if (temp.EditStatus == ((int)MiicYesNoSetting.Yes).ToString() && publishInfo.EditStatus == ((int)MiicYesNoSetting.No).ToString())
            {
                //积分
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = publishInfo.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Publish).ToString(),
                    Score = ScoreConfig.Score.PublishScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = temp.CreaterID,
                    UserName = temp.CreaterName
                }, out message));
            }

            sqls.Add(DBService.UpdateSql<PublishInfo>(publishInfo, out message));

            try
            {
                result = dbService.excuteSqls(sqls, out message);
            }
            catch (Exception ex)
            {
                Config.IlogicLogService.Write(new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                });
            }
            if (result == true)
            {
                DeleteCache(true, o => o.ID == publishInfo.ID, o => o.PublishID == publishInfo.ID);
            }
            return result;
        }

        bool ICommon<PublishInfo>.Delete(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            bool result = false;
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            string message3 = string.Empty;
            string message4 = string.Empty;
            string message5 = string.Empty;
            string message6 = string.Empty;
            string message7 = string.Empty;
            string message8 = string.Empty;
            string message9 = string.Empty;
            List<string> sqls = new List<string>();
            List<AccessoryInfo> accs = GetAccessoryList(id);
            sqls.Add(DBService.DeleteSql<PublishInfo>(new PublishInfo()
            {
                ID = id
            }, out message1));
            if (accs.Count > 0)
            {
                MiicCondition idCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.PublishID),
                    id,
                    DbType.String,
                    MiicDBOperatorSetting.Equal);
                MiicConditionSingle condition = new MiicConditionSingle(idCondition);
                sqls.Add(DBService.DeleteConditionSql<AccessoryInfo>(condition, out message2));
            }

            //收藏级联
            MiicCondition collectPublishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.PublishID),
               id,
               DbType.String,
               MiicDBOperatorSetting.Equal);
            MiicConditionSingle collectCondition = new MiicConditionSingle(collectPublishIDCondition);
            sqls.Add(DBService.DeleteConditionSql<CollectInfo>(collectCondition, out message3));
            //点赞级联
            MiicCondition praisePublishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PraiseInfo, string>(o => o.PublishID),
                id,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            MiicConditionSingle praiseCondition = new MiicConditionSingle(praisePublishIDCondition);
            sqls.Add(DBService.DeleteConditionSql<PraiseInfo>(praiseCondition, out message4));
            //点踩级联
            MiicCondition treadPublishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<TreadInfo, string>(o => o.PublishID),
               id,
               DbType.String,
               MiicDBOperatorSetting.Equal);
            MiicConditionSingle treadCondition = new MiicConditionSingle(treadPublishIDCondition);
            sqls.Add(DBService.DeleteConditionSql<TreadInfo>(treadCondition, out message5));
            //举报级联
            MiicCondition reportPublishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfo, string>(o => o.PublishID),
               id,
               DbType.String,
               MiicDBOperatorSetting.Equal);
            MiicConditionSingle reportCondition = new MiicConditionSingle(reportPublishIDCondition);
            sqls.Add(DBService.DeleteConditionSql<ReportInfo>(reportCondition, out message6));
            //浏览级联
            MiicCondition browsePublishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<BrowseInfo, string>(o => o.PublishID),
               id,
               DbType.String,
               MiicDBOperatorSetting.Equal);
            MiicConditionSingle browseCondition = new MiicConditionSingle(browsePublishIDCondition);
            sqls.Add(DBService.DeleteConditionSql<TreadInfo>(browseCondition, out message7));
            //评论级联
            MiicCondition commentPublishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommentInfo, string>(o => o.PublishID),
               id,
               DbType.String,
               MiicDBOperatorSetting.Equal);
            MiicConditionSingle commentCondition = new MiicConditionSingle(commentPublishIDCondition);
            sqls.Add(DBService.DeleteConditionSql<CommentInfo>(commentCondition, out message8));
            //提醒级联
            MiicCondition noticePublishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<NoticeInfo, string>(o => o.PublishID),
               id,
               DbType.String,
               MiicDBOperatorSetting.Equal);
            MiicConditionSingle noticeCondition = new MiicConditionSingle(noticePublishIDCondition);
            sqls.Add(DBService.DeleteConditionSql<NoticeInfo>(noticeCondition, out message9));

            PublishInfo temp = ((ICommon<PublishInfo>)this).GetInformation(id);
            //积分
            if (temp.EditStatus == ((int)MiicYesNoSetting.No).ToString())
            {
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = temp.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Publish).ToString(),
                    Score = -1 * ScoreConfig.Score.PublishScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = temp.CreaterID,
                    UserName = temp.CreaterName
                }, out message));
            }

            try
            {
                result = dbService.excuteSqls(sqls, out message);
            }
            catch (Exception ex)
            {
                Config.IlogicLogService.Write(new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                });
            }
            if (result == true)
            {
                if (accs.Count > 0)
                {
                    foreach (AccessoryInfo acc in accs.AsEnumerable())
                    {
                        File.Delete(HttpContext.Current.Server.MapPath(acc.FilePath));
                        if (acc.FileType == ((int)AccFileTypeSetting.Photo).ToString())
                        {
                            File.Delete(HttpContext.Current.Server.MapPath("/file/temp/Moments/Photo/" + Path.GetFileName(acc.FilePath)));
                        }
                        else
                        {
                            File.Delete(HttpContext.Current.Server.MapPath("/file/temp/Moments/File/" + Path.GetFileName(acc.FilePath)));
                        }
                    }
                    DeleteCache(true, o => o.ID == id, o => o.PublishID == id);
                }
                else
                {
                    DeleteCache(false, o => o.ID == id);
                }
                PraiseInfoDao.DeleteCacheByPublishID(id);
                TreadInfoDao.DeleteCacheByPublishID(id);
                CollectInfoDao.DeleteCacheByPublishID(id);
                BrowseInfoDao.DeleteCacheByPublishID(id);
                CommentInfoDao.DeleteCacheByPublishID(id);
            }
            return result;
        }

        PublishInfo ICommon<PublishInfo>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            PublishInfo result = null;
            string message = string.Empty;
            try
            {
                result = items.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new PublishInfo
                    {
                        ID = id
                    }, out message);
                    if (result != null)
                    {
                        InsertCache(result);
                    }
                }
                else
                {
                    string serializer = Config.Serializer.Serialize(result);
                    result = Config.Serializer.Deserialize<PublishInfo>(serializer);
                }
            }
            catch (Exception ex)
            {
                Config.IlogicLogService.Write(new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                });
            }
            return result;
        }

        /// <summary>
        /// 获取某人朋友圈发布信息（支持我的/某人）
        /// </summary>
        /// <param name="dateView">日期视图</param>
        /// <param name="page">分页，默认不分页</param>
        /// <returns>发布信息列表</returns>
        public DataTable GetPersonMomentsPublishInfos(GeneralDateView dateView, MiicPage page)
        {
            Contract.Requires<ArgumentNullException>(dateView != null, "参数dateView:不能为空");
            DataTable result = new DataTable();
            string message = string.Empty;
            try
            {
                if (page == null)
                {
                    throw new Exception("暂不支持不分页查询");
                }
                else
                {
                    Dictionary<String, String> paras = new Dictionary<String, String>();
                    paras.Add("YEAR", dateView.Year);
                    paras.Add("MONTH", dateView.Month);
                    paras.Add("USER_ID", dateView.userID);
                    paras.Add("PAGE_START", page.pageStart);
                    paras.Add("PAGE_END", page.pageEnd);
                    string storeProcedureName = string.Empty;
                    if (dateView is MyDateView)
                    {
                        storeProcedureName = "GetMyAddressMomentsSearchResult";
                    }
                    else if (dateView is PersonDateView)
                    {
                        storeProcedureName = "GetPersonSearchResult";
                    }
                    else
                    {
                        throw new Exception("参数异常，未知的DateView视图");
                    }
                    result = dbService.QueryStoredProcedure<string>(storeProcedureName, paras, out message);
                }
            }
            catch (Exception ex)
            {
                LogicLog log = new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                };
                Config.IlogicLogService.Write(log);
            }
            return result;
        }

        /// <summary>
        /// 获取某人朋友圈发布信息数（支持我的/某人）
        /// </summary>
        /// <param name="dateView">日期视图</param>
        /// <returns>发布信息列表数</returns>
        public int GetPersonMomentsPublishCount(GeneralDateView dateView)
        {
            Contract.Requires<ArgumentNullException>(dateView != null, "参数dateView:不能为空");
            int result = 0;
            string message = string.Empty;
            string sql = "select count(ID) FROM GetMomentsAddressPublishInfo('" + dateView.userID + "') ";

            //对查询人可见
            sql += " WHERE CAN_SEE_ADDRESSER = '" + ((int)MiicYesNoSetting.Yes).ToString() + "' ";
            //不在查询人黑名单
            sql += " AND IS_BLACK_LIST = '" + ((int)MiicYesNoSetting.No).ToString() + "' ";
            //已发表状态
            sql += " AND EDIT_STATUS = '" + ((int)MiicYesNoSetting.No).ToString() + "' ";

            if (dateView is PersonDateView)
            {
                //发表人是用户
                sql += " AND IS_FRIEND = '" + ((int)MiicYesNoSetting.No).ToString() + "' ";
                sql += " AND YEAR(PUBLISH_TIME) = '" + ((PersonDateView)dateView).Year + "' ";
                if (!string.IsNullOrEmpty(((PersonDateView)dateView).Month))
                {
                    sql += " AND MONTH(PUBLISH_TIME) = '" + ((PersonDateView)dateView).Month + "' ";
                }
            }
            else
            {
                sql += " AND YEAR(PUBLISH_TIME) = '" + ((MyDateView)dateView).Year + "' ";
                if (!string.IsNullOrEmpty(((MyDateView)dateView).Month))
                {
                    sql += " AND MONTH(PUBLISH_TIME) = '" + ((MyDateView)dateView).Month + "' ";
                }
            }

            try
            {
                result = dbService.GetSqlCount(sql, out message);
            }
            catch (Exception ex)
            {
                LogicLog log = new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                };
                Config.IlogicLogService.Write(log);
            }
            return result;
        }

        /// <summary>
        /// 获取最早发布的朋友圈信息
        /// </summary>
        /// <param name="topView">条件视图</param>
        /// <returns>发布信息列表</returns>
        public DataTable GetOldestMomentsPubilishInfos(GeneralTopView topView)
        {
            Contract.Requires<ArgumentOutOfRangeException>(topView != null, "参数topView不能为空");
            Contract.Requires<ArgumentOutOfRangeException>(topView.Top > 0, "参数topView.Top:必须为正整数");
            DataTable result = new DataTable();
            MiicColumnCollections column = new MiicColumnCollections();
            string message = string.Empty;
            string sql = string.Empty;
            sql += "with INFO_PAGE as (";
            sql += "select dense_rank() over ( ORDER BY  Temp.PUBLISH_TIME DESC) as row,Temp.* ";
            sql += "from ( ";
            sql += "SELECT ";
            sql += " INFO_LIST.*,";
            sql += " dbo.MOMENTS_ACC_INFO.ID as MomentsPublishAccessoryInfoID,";
            sql += " dbo.MOMENTS_ACC_INFO.FILE_NAME,";
            sql += " dbo.MOMENTS_ACC_INFO.FILE_PATH,";
            sql += " dbo.MOMENTS_ACC_INFO.UPLOAD_TIME,";
            sql += " dbo.MOMENTS_ACC_INFO.FILE_TYPE,";
            sql += " REAL_NAME = MIIC_SOCIAL_COMMON.dbo.SIMPLE_USER_VIEW.USER_NAME,";
            sql += " MIIC_SOCIAL_COMMON.dbo.SIMPLE_USER_VIEW.ORG_NAME,";
            sql += " MIIC_SOCIAL_COMMON.dbo.SIMPLE_USER_VIEW.MICRO_USER_URL,";
            sql += " MIIC_SOCIAL_COMMON.dbo.SIMPLE_USER_VIEW.USER_TYPE,";
            sql += " dbo.MOMENTS_PRAISE_INFO.PRAISER_ID, ";
            sql += " dbo.MOMENTS_PRAISE_INFO.PRAISER_NAME, ";
            sql += " dbo.MOMENTS_TREAD_INFO.TREADER_ID, ";
            sql += " dbo.MOMENTS_TREAD_INFO.TREADER_NAME, ";
            sql += " dbo.MOMENTS_REPORT_INFO.REPORTER_ID, ";
            sql += " dbo.MOMENTS_REPORT_INFO.REPORTER_NAME,";
            sql += " dbo.MOMENTS_COLLECT_INFO.COLLECTOR_ID,";
            sql += " dbo.MOMENTS_COLLECT_INFO.COLLECTOR_NAME,";
            sql += " dbo.MOMENTS_COLLECT_INFO.COLLECT_VALID";
            sql += " FROM GetMomentsAddressPublishInfo('" + topView.userID + "') INFO_LIST ";
            sql += " LEFT JOIN dbo.MOMENTS_ACC_INFO ON INFO_LIST.ID=dbo.MOMENTS_ACC_INFO.PUBLISH_ID";
            sql += " LEFT JOIN MIIC_SOCIAL_COMMON.dbo.SIMPLE_USER_VIEW ON INFO_LIST.CREATER_ID=MIIC_SOCIAL_COMMON.dbo.SIMPLE_USER_VIEW.USER_ID";
            sql += " LEFT JOIN dbo.MOMENTS_PRAISE_INFO ON INFO_LIST.ID=dbo.MOMENTS_PRAISE_INFO.PUBLISH_ID";
            sql += " LEFT JOIN dbo.MOMENTS_TREAD_INFO ON INFO_LIST.ID=dbo.MOMENTS_TREAD_INFO.PUBLISH_ID";
            sql += " LEFT JOIN dbo.MOMENTS_REPORT_INFO ON INFO_LIST.ID=dbo.MOMENTS_REPORT_INFO.PUBLISH_ID";
            sql += " LEFT JOIN dbo.MOMENTS_COLLECT_INFO ON INFO_LIST.ID=dbo.MOMENTS_COLLECT_INFO.PUBLISH_ID";
            //对查询人可见
            sql += " WHERE CAN_SEE_ADDRESSER = '" + ((int)MiicYesNoSetting.Yes).ToString() + "' ";
            //不在查询人黑名单
            sql += " AND IS_BLACK_LIST = '" + ((int)MiicYesNoSetting.No).ToString() + "' ";
            //已发表状态
            sql += " AND EDIT_STATUS = '" + ((int)MiicYesNoSetting.No).ToString() + "' ";
            //发表人是查询者
            sql += " AND IS_FRIEND = '" + ((int)MiicYesNoSetting.No).ToString() + "' ";

            //默认时间正序排列
            sql += " ) AS Temp)";
            sql += "SELECT * FROM INFO_PAGE WHERE row between 1 and " + topView.Top + " ";
            try
            {
                result = dbService.querySql(sql, out message);
            }
            catch (Exception ex)
            {
                LogicLog log = new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                };
                Config.IlogicLogService.Write(log);
            }
            return result;
        }

        /// <summary>
        /// 获取年份列表
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<string> GetPersonMomentsPublishInfosYearList(GeneralDateView dateView)
        {
            Contract.Requires<ArgumentNullException>(dateView != null, "参数dateView:不能为空");
            List<string> result = new List<string>();
            MiicColumnCollections column = new MiicColumnCollections(new MiicDistinct());
            string message = string.Empty;
            string sql = "select DISTINCT YEAR(PUBLISH_TIME) FROM GetMomentsAddressPublishInfo('" + dateView.userID + "') ";
            //对查询人可见
            sql += " WHERE CAN_SEE_ADDRESSER = '" + ((int)MiicYesNoSetting.Yes).ToString() + "' ";
            //不在查询人黑名单
            sql += " AND IS_BLACK_LIST = '" + ((int)MiicYesNoSetting.No).ToString() + "' ";
            //已发表状态
            sql += " AND EDIT_STATUS = '" + ((int)MiicYesNoSetting.No).ToString() + "' ";

            if (dateView is PersonDateView)
            {
                //发表人是用户
                sql += " AND IS_FRIEND = '" + ((int)MiicYesNoSetting.No).ToString() + "' ";
            }

            //默认时间倒序排列
            sql += " ORDER BY YEAR(PUBLISH_TIME) DESC";

            try
            {
                DataTable dt = dbService.querySql(sql, out message);
                if (dt.Rows.Count != 0)
                {
                    foreach (var dr in dt.AsEnumerable())
                    {
                        result.Add(dr[0].ToString());
                    }
                    result = result.Distinct().ToList();
                }
            }
            catch (Exception ex)
            {
                LogicLog log = new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                };
                Config.IlogicLogService.Write(log);
            }

            return result;
        }
        /// <summary>
        /// 获取月份列表
        /// </summary>
        /// <param name="year"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<string> GetPersonMomentsPublishInfosMonthList(GeneralDateView dateView)
        {
            Contract.Requires<ArgumentNullException>(dateView != null, "参数dateView:不能为空");
            List<string> result = new List<string>();
            MiicColumnCollections column = new MiicColumnCollections(new MiicDistinct());
            string message = string.Empty;
            string sql = "select DISTINCT MONTH(PUBLISH_TIME) FROM GetMomentsAddressPublishInfo('" + dateView.userID + "') ";

            //对查询人可见
            sql += " WHERE CAN_SEE_ADDRESSER = '" + ((int)MiicYesNoSetting.Yes).ToString() + "' ";
            //不在查询人黑名单
            sql += " AND IS_BLACK_LIST = '" + ((int)MiicYesNoSetting.No).ToString() + "' ";
            //已发表状态
            sql += " AND EDIT_STATUS = '" + ((int)MiicYesNoSetting.No).ToString() + "' ";

            if (dateView is PersonDateView)
            {
                //发表人是用户
                sql += " AND IS_FRIEND = '" + ((int)MiicYesNoSetting.No).ToString() + "' ";
                sql += " AND YEAR(PUBLISH_TIME) = '" + ((PersonDateView)dateView).Year + "' ";
            }
            else
            {
                sql += " AND YEAR(PUBLISH_TIME) = '" + ((MyDateView)dateView).Year + "' ";
            }

            //默认时间倒序排列
            sql += " ORDER BY MONTH(PUBLISH_TIME) DESC";
            try
            {
                DataTable dt = dbService.querySql(sql, out message);
                if (dt != null && dt.Rows.Count != 0)
                {
                    foreach (var dr in dt.AsEnumerable())
                    {
                        result.Add(dr[0].ToString());
                    }
                    result = result.Distinct().ToList();
                }
            }
            catch (Exception ex)
            {
                LogicLog log = new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                };
                Config.IlogicLogService.Write(log);
            }

            return result;
        }

        /// <summary>
        /// 获取某人最新朋友圈信息（条件：长篇、已发布、有效的、上线的）
        /// </summary>
        /// <param name="top">默认：15</param>
        /// <returns>热门博文列表</returns>
        public DataTable GetTopSimpleMomentsInfos(string userID, int top)
        {
            Contract.Requires<ArgumentOutOfRangeException>(top > 0, "参数top:不能为负");
            string message = string.Empty;
            DataTable result = new DataTable();
            MiicColumnCollections columns = new MiicColumnCollections(new MiicTop(top));
            MiicColumn userUrlColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<Miic.Manage.User.MiicSocialUserInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.MiicSocialUserInfo, string>(o => o.MicroUserUrl));
            columns.Add(userUrlColumn);
            MiicColumn publishAllColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>());
            columns.Add(publishAllColumn);
            MiicRelationCollections relation = new MiicRelationCollections(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>());
            MiicFriendRelation userRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.CreaterID),
                Config.Attribute.GetSqlTableNameByClassName<Miic.Manage.User.MiicSocialUserInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.MiicSocialUserInfo, string>(o => o.ID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.InnerJoin);
            relation.Add(userRelation);
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition microTypeCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.PublishType),
                (int)PublishInfoTypeSetting.Long,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, microTypeCondition));
            MiicCondition editStatusCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.EditStatus),
                (int)MiicYesNoSetting.No,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(editStatusCondition));
            MiicCondition createrCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<PublishInfo, string>(o => o.CreaterID),
                userID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(createrCondition));
            List<MiicOrderBy> orders = new List<MiicOrderBy>();
            orders.Add(new MiicOrderBy()
            {
                Desc = true,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.PublishTime)
            });
            try
            {
                result = dbService.GetInformations(columns, relation, condition, out message);
            }
            catch (Exception ex)
            {
                LogicLog log = new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                };
                Config.IlogicLogService.Write(log);
            }
            return result;
        }


        /// <summary>
        /// 获取最新发布的朋友圈信息
        /// </summary>
        /// <param name="topView">条件视图</param>
        /// <returns>发布信息列表</returns>
        public DataTable GetNewestMomentsPublishInfos(GeneralTopView topView)
        {
            Contract.Requires<ArgumentOutOfRangeException>(topView != null, "参数topView不能为空");
            Contract.Requires<ArgumentOutOfRangeException>(topView.Top > 0, "参数topView.Top:必须为正整数");
            DataTable result = new DataTable();
            MiicColumnCollections column = new MiicColumnCollections();
            string message = string.Empty;
            string sql = string.Empty;
            sql += "with INFO_PAGE as (";
            sql += "select dense_rank() over ( ORDER BY  Temp.PUBLISH_TIME ASC) as row,Temp.* ";
            sql += "from ( ";
            sql += "SELECT ";
            sql += " INFO_LIST.*,";
            sql += " dbo.MOMENTS_ACC_INFO.ID as MomentsPublishAccessoryInfoID,";
            sql += " dbo.MOMENTS_ACC_INFO.FILE_NAME,";
            sql += " dbo.MOMENTS_ACC_INFO.FILE_PATH,";
            sql += " dbo.MOMENTS_ACC_INFO.UPLOAD_TIME,";
            sql += " dbo.MOMENTS_ACC_INFO.FILE_TYPE,";
            sql += " REAL_NAME = MIIC_SOCIAL_COMMON.dbo.SIMPLE_USER_VIEW.USER_NAME,";
            sql += " MIIC_SOCIAL_COMMON.dbo.SIMPLE_USER_VIEW.ORG_NAME,";
            sql += " MIIC_SOCIAL_COMMON.dbo.SIMPLE_USER_VIEW.MICRO_USER_URL,";
            sql += " MIIC_SOCIAL_COMMON.dbo.SIMPLE_USER_VIEW.USER_TYPE,";
            sql += " dbo.MOMENTS_PRAISE_INFO.PRAISER_ID, ";
            sql += " dbo.MOMENTS_PRAISE_INFO.PRAISER_NAME, ";
            sql += " dbo.MOMENTS_TREAD_INFO.TREADER_ID, ";
            sql += " dbo.MOMENTS_TREAD_INFO.TREADER_NAME, ";
            sql += " dbo.MOMENTS_REPORT_INFO.REPORTER_ID, ";
            sql += " dbo.MOMENTS_REPORT_INFO.REPORTER_NAME,";
            sql += " dbo.MOMENTS_COLLECT_INFO.COLLECTOR_ID,";
            sql += " dbo.MOMENTS_COLLECT_INFO.COLLECTOR_NAME,";
            sql += " dbo.MOMENTS_COLLECT_INFO.COLLECT_VALID";
            sql += " FROM GetMomentsAddressPublishInfo('" + topView.userID + "') INFO_LIST ";
            sql += " LEFT JOIN dbo.MOMENTS_ACC_INFO ON INFO_LIST.ID=dbo.MOMENTS_ACC_INFO.PUBLISH_ID";
            sql += " LEFT JOIN MIIC_SOCIAL_COMMON.dbo.SIMPLE_USER_VIEW ON INFO_LIST.CREATER_ID=MIIC_SOCIAL_COMMON.dbo.SIMPLE_USER_VIEW.USER_ID";
            sql += " LEFT JOIN dbo.MOMENTS_PRAISE_INFO ON INFO_LIST.ID=dbo.MOMENTS_PRAISE_INFO.PUBLISH_ID";
            sql += " LEFT JOIN dbo.MOMENTS_TREAD_INFO ON INFO_LIST.ID=dbo.MOMENTS_TREAD_INFO.PUBLISH_ID";
            sql += " LEFT JOIN dbo.MOMENTS_REPORT_INFO ON INFO_LIST.ID=dbo.MOMENTS_REPORT_INFO.PUBLISH_ID";
            sql += " LEFT JOIN dbo.MOMENTS_COLLECT_INFO ON INFO_LIST.ID=dbo.MOMENTS_COLLECT_INFO.PUBLISH_ID";
            //对查询人可见
            sql += " WHERE CAN_SEE_ADDRESSER = '" + ((int)MiicYesNoSetting.Yes).ToString() + "' ";
            //不在查询人黑名单
            sql += " AND IS_BLACK_LIST = '" + ((int)MiicYesNoSetting.No).ToString() + "' ";
            //已发表状态
            sql += " AND EDIT_STATUS = '" + ((int)MiicYesNoSetting.No).ToString() + "' ";
            //发表人是查询者
            sql += " AND IS_FRIEND = '" + ((int)MiicYesNoSetting.No).ToString() + "' ";

            //默认时间正序排列
            sql += " ) AS Temp)";
            sql += "SELECT * FROM INFO_PAGE WHERE row between 1 and " + topView.Top + " ";
            try
            {
                result = dbService.querySql(sql, out message);
            }
            catch (Exception ex)
            {
                LogicLog log = new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                };
                Config.IlogicLogService.Write(log);
            }
            return result;
        }

        /// <summary>
        /// 根据ID获取详细信息内容
        /// </summary>
        /// <param name="ID">信息ID</param>
        /// <returns>详细信息</returns>
        public DataTable GetDetailPublishInfo(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            string message = string.Empty;
            DataTable result = new DataTable();
            MiicCondition idCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<PublishInfo, string>(o => o.ID),
                id,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            MiicConditionSingle condition = new MiicConditionSingle(idCondition);
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID),
               Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.PublishID),
               MiicDBOperatorSetting.Equal,
               MiicDBRelationSetting.LeftJoin);
            MiicColumnCollections column = new MiicColumnCollections();
            MiicColumn microColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>());
            column.Add(microColumn);
            MiicColumn microAccessoryIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                string.Empty,
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID),
                "MomentsPublishAccessoryInfoID");
            column.Add(microAccessoryIDColumn);
            MiicColumn fileNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileName));
            column.Add(fileNameColumn);
            MiicColumn filePathColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FilePath));
            column.Add(filePathColumn);
            MiicColumn uploadTime = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, DateTime?>(o => o.UploadTime));
            column.Add(uploadTime);
            MiicColumn fileTypeColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileType));
            column.Add(fileTypeColumn);
            try
            {
                result = dbService.GetInformations(column, relation, condition, out message);
            }
            catch (Exception ex)
            {
                Config.IlogicLogService.Write(new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                });
            }
            return result;
        }
        /// <summary>
        /// 设置已经发布的微博上下线
        /// </summary>
        /// <param name="ID">上下线状态视图</param>
        /// <returns>Yes/No</returns>
        public bool SetEditStatus(EditStatusView editStatusView)
        {
            Contract.Requires<ArgumentNullException>(editStatusView != null, "参数editStatusView:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(editStatusView.ID), "参数editStatusView.ID:不能为空");
            bool result = false;
            string message = string.Empty;
            int count = 0;
            try
            {
                result = dbService.Update<PublishInfo>(new PublishInfo()
                {
                    ID = editStatusView.ID,
                    UpdateTime = DateTime.Now,
                    EditStatus = ((int)editStatusView.EditStatus).ToString()
                }, out count, out message);
            }
            catch (Exception ex)
            {
                Config.IlogicLogService.Write(new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                });
            }
            if (result == true)
            {
                DeleteCache(true, o => o.ID == editStatusView.ID, o => o.PublishID == editStatusView.ID);
            }
            return result;
        }



        /// <summary>
        /// 新增信息
        /// </summary>
        /// <param name="publishInfo">信息</param>
        /// <param name="noticeUserView">提醒人视图</param>
        /// <returns>Yes/No</returns>
        public bool Insert(PublishInfo publishInfo, NoticeUserView noticeUserView)
        {
            Contract.Requires<ArgumentNullException>(publishInfo != null, "参数publishInfo:不能为空");
            Contract.Requires<ArgumentNullException>(noticeUserView != null, "参数noticeUserView:不能为空");
            bool result = false;
            List<string> sqls = new List<string>();
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            /*------------------------------朋友圈初始化值-----------------------------------*/
            //浏览总数
            publishInfo.BrowseNum = 0;
            //点赞数
            publishInfo.PraiseNum = 0;
            //踩数
            publishInfo.TreadNum = 0;
            //转发数
            publishInfo.TransmitNum = 0;
            //被评论数
            publishInfo.CommentNum = 0;
            //被举报数
            publishInfo.ReportNum = 0;
            //收藏总数
            publishInfo.CollectNum = 0;
            //微博是否有附件
            publishInfo.HasAcc = ((int)MiicYesNoSetting.No).ToString();
            /*------------------------------用户初始化值-----------------------------------*/

            sqls.Add(DBService.InsertSql(publishInfo, out message1));


            //积分
            if (publishInfo.EditStatus == ((int)MiicYesNoSetting.No).ToString())
            {
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = publishInfo.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Publish).ToString(),
                    Score = ScoreConfig.Score.PublishScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = publishInfo.CreaterID,
                    UserName = publishInfo.CreaterName
                }, out message));
            }

            foreach (var item in noticeUserView.Noticers.AsEnumerable())
            {
                sqls.Add(DBService.InsertSql<NoticeInfo>(new NoticeInfo()
                {
                    ID = Guid.NewGuid().ToString(),
                    NoticerID = item.UserID,
                    NoticerName = item.UserName,
                    Source = ((int)noticeUserView.NoticeSource).ToString(),
                    NoticeType = ((int)noticeUserView.NoticeType).ToString(),
                    PublisherID = publishInfo.CreaterID,
                    PublisherName = publishInfo.CreaterName,
                    PublishID = publishInfo.ID,
                    PublishTime = publishInfo.PublishTime,
                    ReadStatus = ((int)MiicReadStatusSetting.UnRead).ToString()
                }, out message2));
            }

            try
            {
                result = dbService.excuteSqls(sqls, out message);
            }
            catch (Exception ex)
            {
                Config.IlogicLogService.Write(new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                });
            }
            if (result == true)
            {
                InsertCache(publishInfo);
            }
            return result;
        }
        /// <summary>
        /// 新增信息
        /// </summary>
        /// <param name="publishInfo">信息</param>
        /// <param name="noticeUserView">提醒人视图</param>
        /// <param name="publishAccessoryInfo">信息附件</param>
        /// <returns>Yes/No</returns>
        public bool Insert(PublishInfo publishInfo, NoticeUserView noticeUserView, List<AccessoryInfo> publishAccessoryInfos)
        {
            Contract.Requires<ArgumentNullException>(publishInfo != null, "参数publishInfo:不能为空");
            Contract.Requires<ArgumentNullException>(publishAccessoryInfos != null && publishAccessoryInfos.Count != 0, "参数publishAccessoryInfos:不能为空");
            bool result = false;
            List<string> sqls = new List<string>();
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            /*------------------------------朋友圈初始化值-----------------------------------*/
            //浏览总数
            publishInfo.BrowseNum = 0;
            //点赞数
            publishInfo.PraiseNum = 0;
            //踩数
            publishInfo.TreadNum = 0;
            //转发数
            publishInfo.TransmitNum = 0;
            //被评论数
            publishInfo.CommentNum = 0;
            //被举报数
            publishInfo.ReportNum = 0;
            //收藏总数
            publishInfo.CollectNum = 0;
            //微博是否有附件
            publishInfo.HasAcc = ((int)MiicYesNoSetting.Yes).ToString();
            /*------------------------------用户初始化值-----------------------------------*/

            sqls.Add(DBService.InsertSql(publishInfo, out message1));


            //积分
            if (publishInfo.EditStatus == ((int)MiicYesNoSetting.No).ToString())
            {
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = publishInfo.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Publish).ToString(),
                    Score = ScoreConfig.Score.PublishScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = publishInfo.CreaterID,
                    UserName = publishInfo.CreaterName
                }, out message));
            }

            if (noticeUserView != null)
            {
                foreach (var item in noticeUserView.Noticers.AsEnumerable())
                {
                    sqls.Add(DBService.InsertSql<NoticeInfo>(new NoticeInfo()
                    {
                        ID = Guid.NewGuid().ToString(),
                        NoticerID = item.UserID,
                        NoticerName = item.UserName,
                        Source = ((int)noticeUserView.NoticeSource).ToString(),
                        NoticeType = ((int)noticeUserView.NoticeType).ToString(),
                        PublisherID = publishInfo.CreaterID,
                        PublisherName = publishInfo.CreaterName,
                        PublishID = publishInfo.ID,
                        PublishTime = publishInfo.PublishTime,
                        ReadStatus = ((int)MiicReadStatusSetting.UnRead).ToString()
                    }, out message2));
                }

            }
            foreach (var item in publishAccessoryInfos.AsEnumerable())
            {
                item.FilePath = GetRealSavePath(item.FilePath);
                sqls.Add(DBService.InsertSql(item, out message2));
            }
            //附件处理
            bool fileResult = false;
            try
            {
                try
                {
                    foreach (var item in publishAccessoryInfos)
                    {
                        string dest = HttpContext.Current.Server.MapPath(item.FilePath);
                        string source = string.Empty;
                        if (item.FileType == ((int)AccFileTypeSetting.Photo).ToString())
                        {
                            source = HttpContext.Current.Server.MapPath("/file/temp/PublishInfoAcc/Photo/" + Path.GetFileName(dest));
                        }
                        else
                        {
                            source = HttpContext.Current.Server.MapPath("/file/temp/PublishInfoAcc/File/" + Path.GetFileName(dest));
                        }
                        File.Move(source, dest);
                    }
                    fileResult = true;
                }
                catch (Exception ex)
                {
                    Config.IlogicLogService.Write(new LogicLog()
                    {
                        AppName = Config.AppName,
                        ClassName = ClassName,
                        NamespaceName = NamespaceName,
                        MethodName = MethodBase.GetCurrentMethod().Name,
                        Message = ex.Message,
                        Oper = Config.Oper
                    });
                }

                if (fileResult == true)
                {
                    result = dbService.excuteSqls(sqls, out message);
                }
            }
            catch (Exception ex)
            {
                Config.IlogicLogService.Write(new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                });
            }
            if (result == true)
            {
                InsertCache(publishInfo, publishAccessoryInfos);
            }
            return result;
        }
        /// <summary>
        /// 更新信息
        /// </summary>
        /// <param name="publishInfo">信息</param>
        /// <param name="removeSimpleAccessoryViews">删除附件队列</param>
        /// <param name="noticeUserView">提醒人视图,可为空</param>
        /// <returns>Yes/No</returns>
        public bool Update(PublishInfo publishInfo, List<SimpleAccessoryView> removeSimpleAccessoryViews, NoticeUserView noticeUserView)
        {
            Contract.Requires<ArgumentNullException>(publishInfo != null, "参数publishInfo:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(publishInfo.ID), "参数publishInfo.ID:不能为空");
            Contract.Requires<ArgumentNullException>(removeSimpleAccessoryViews != null && removeSimpleAccessoryViews.Count != 0, "参数removeSimpleAccessoryViews:不能为空");
            bool result = false;
            List<string> sqls = new List<string>();
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            string message3 = string.Empty;
            try
            {
                PublishInfo tempPublishInfo = ((ICommon<PublishInfo>)this).GetInformation(publishInfo.ID);

                if (tempPublishInfo.EditStatus == ((int)MiicYesNoSetting.No).ToString() && publishInfo.EditStatus == ((int)MiicYesNoSetting.Yes).ToString())
                {
                    //积分
                    sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                    {
                        ID = Guid.NewGuid().ToString(),
                        BusinessID = publishInfo.ID,
                        CreateTime = DateTime.Now,
                        GetWay = ((int)GetWayTypeSetting.Publish).ToString(),
                        Score = -1 * ScoreConfig.Score.PublishScore,
                        ServiceID = ScoreConfig.ServiceID,
                        UserID = tempPublishInfo.CreaterID,
                        UserName = tempPublishInfo.CreaterName
                    }, out message));
                }

                if (tempPublishInfo.EditStatus == ((int)MiicYesNoSetting.Yes).ToString() && publishInfo.EditStatus == ((int)MiicYesNoSetting.No).ToString())
                {
                    //积分
                    sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                    {
                        ID = Guid.NewGuid().ToString(),
                        BusinessID = publishInfo.ID,
                        CreateTime = DateTime.Now,
                        GetWay = ((int)GetWayTypeSetting.Publish).ToString(),
                        Score = ScoreConfig.Score.PublishScore,
                        ServiceID = ScoreConfig.ServiceID,
                        UserID = tempPublishInfo.CreaterID,
                        UserName = tempPublishInfo.CreaterName
                    }, out message));
                }

                foreach (var item in removeSimpleAccessoryViews)
                {
                    sqls.Add(DBService.DeleteSql<AccessoryInfo>(new AccessoryInfo()
                    {
                        ID = item.ID
                    }, out message1));
                }

                List<AccessoryInfo> temp = this.GetAccessoryList(publishInfo.ID);
                if (temp.Count == removeSimpleAccessoryViews.Count)
                {
                    publishInfo.HasAcc = ((int)MiicYesNoSetting.No).ToString();
                }
                sqls.Add(DBService.UpdateSql<PublishInfo>(publishInfo, out message2));

                if (noticeUserView != null)
                {
                    //@人员
                    foreach (var item in noticeUserView.Noticers.AsEnumerable())
                    {
                        sqls.Add(DBService.InsertSql<NoticeInfo>(new NoticeInfo()
                        {
                            ID = Guid.NewGuid().ToString(),
                            NoticerID = item.UserID,
                            NoticerName = item.UserName,
                            Source = ((int)noticeUserView.NoticeSource).ToString(),
                            NoticeType = ((int)noticeUserView.NoticeType).ToString(),
                            PublisherID = tempPublishInfo.CreaterID,
                            PublisherName = tempPublishInfo.CreaterName,
                            PublishID = tempPublishInfo.ID,
                            PublishTime = (publishInfo.PublishTime == null ? tempPublishInfo.PublishTime : publishInfo.PublishTime),
                            ReadStatus = ((int)MiicReadStatusSetting.UnRead).ToString()
                        }, out message3));
                    }
                }

                bool fileResult = false;
                try
                {
                    foreach (var item in removeSimpleAccessoryViews)
                    {
                        File.Delete(HttpContext.Current.Server.MapPath(item.FilePath));
                        if (item.FileType == ((int)AccFileTypeSetting.Photo).ToString())
                        {
                            File.Delete(HttpContext.Current.Server.MapPath("/file/temp/PublishInfoAcc/Photo/" + Path.GetFileName(item.FilePath)));
                        }
                        else
                        {
                            File.Delete(HttpContext.Current.Server.MapPath("/file/temp/PublishInfoAcc/File/" + Path.GetFileName(item.FilePath)));
                        }
                    }

                    fileResult = true;
                }
                catch (Exception ex)
                {
                    Config.IlogicLogService.Write(new LogicLog()
                    {
                        AppName = Config.AppName,
                        ClassName = ClassName,
                        NamespaceName = NamespaceName,
                        MethodName = MethodBase.GetCurrentMethod().Name,
                        Message = ex.Message,
                        Oper = Config.Oper
                    });
                }


                if (fileResult == true)
                {
                    result = dbService.excuteSqls(sqls, out message);
                }
            }
            catch (Exception ex)
            {
                Config.IlogicLogService.Write(new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                });
            }
            if (result == true)
            {
                DeleteCache(true, o => o.ID == publishInfo.ID, o => o.PublishID == publishInfo.ID);
            }
            return result;
        }
        /// <summary>
        /// 更新信息
        /// </summary>
        /// <param name="publishInfo">信息</param>
        /// <param name="publishAccessoryInfos">信息附件</param>
        /// <param name="removeSimpleAccessoryViews">删除附件队列</param>
        /// <param name="noticeUserView">提醒人视图，可为空</param>
        /// <returns></returns>
        public bool Update(PublishInfo publishInfo, List<AccessoryInfo> publishAccessoryInfos, List<SimpleAccessoryView> removeSimpleAccessoryViews, NoticeUserView noticeUserView)
        {
            Contract.Requires<ArgumentNullException>(publishInfo != null, "参数publishInfo:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(publishInfo.ID), "参数publishInfo.ID:不能为空");
            Contract.Requires<ArgumentNullException>(publishAccessoryInfos != null && publishAccessoryInfos.Count != 0, "参数publishAccessoryInfos:不能为空");
            bool result = false;
            List<string> sqls = new List<string>();
            string message = string.Empty,
                   message1 = string.Empty,
                   message2 = string.Empty,
                   message3 = string.Empty,
                   message4 = string.Empty;
            try
            {
                PublishInfo tempPublishInfo = ((ICommon<PublishInfo>)this).GetInformation(publishInfo.ID);

                if (tempPublishInfo.EditStatus == ((int)MiicYesNoSetting.No).ToString() && publishInfo.EditStatus == ((int)MiicYesNoSetting.Yes).ToString())
                {
                    //积分
                    sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                    {
                        ID = Guid.NewGuid().ToString(),
                        BusinessID = publishInfo.ID,
                        CreateTime = DateTime.Now,
                        GetWay = ((int)GetWayTypeSetting.Publish).ToString(),
                        Score = -1 * ScoreConfig.Score.PublishScore,
                        ServiceID = ScoreConfig.ServiceID,
                        UserID = tempPublishInfo.CreaterID,
                        UserName = tempPublishInfo.CreaterName
                    }, out message));
                }

                if (tempPublishInfo.EditStatus == ((int)MiicYesNoSetting.Yes).ToString() && publishInfo.EditStatus == ((int)MiicYesNoSetting.No).ToString())
                {
                    //积分
                    sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                    {
                        ID = Guid.NewGuid().ToString(),
                        BusinessID = publishInfo.ID,
                        CreateTime = DateTime.Now,
                        GetWay = ((int)GetWayTypeSetting.Publish).ToString(),
                        Score = ScoreConfig.Score.PublishScore,
                        ServiceID = ScoreConfig.ServiceID,
                        UserID = tempPublishInfo.CreaterID,
                        UserName = tempPublishInfo.CreaterName
                    }, out message));
                }

                if (publishAccessoryInfos.Count != 0 && tempPublishInfo.HasAcc == ((int)MiicYesNoSetting.No).ToString())
                {
                    publishInfo.HasAcc = ((int)MiicYesNoSetting.Yes).ToString();
                }

                sqls.Add(DBService.UpdateSql<PublishInfo>(publishInfo, out message1));


                foreach (var item in publishAccessoryInfos.AsEnumerable())
                {
                    item.FilePath = GetRealSavePath(item.FilePath);
                    sqls.Add(DBService.InsertSql(item, out message2));
                }

                if (removeSimpleAccessoryViews != null && removeSimpleAccessoryViews.Count != 0)
                {
                    foreach (var item in removeSimpleAccessoryViews)
                    {
                        sqls.Add(DBService.DeleteSql<AccessoryInfo>(new AccessoryInfo()
                        {
                            ID = item.ID
                        }, out message3));
                    }
                }

                if (noticeUserView != null)
                {
                    //@人员
                    foreach (var item in noticeUserView.Noticers.AsEnumerable())
                    {
                        sqls.Add(DBService.InsertSql<NoticeInfo>(new NoticeInfo()
                        {
                            ID = Guid.NewGuid().ToString(),
                            NoticerID = item.UserID,
                            NoticerName = item.UserName,
                            Source = ((int)noticeUserView.NoticeSource).ToString(),
                            NoticeType = ((int)noticeUserView.NoticeType).ToString(),
                            PublisherID = tempPublishInfo.CreaterID,
                            PublisherName = tempPublishInfo.CreaterName,
                            PublishID = tempPublishInfo.ID,
                            PublishTime = (publishInfo.PublishTime == null ? tempPublishInfo.PublishTime : publishInfo.PublishTime),
                            ReadStatus = ((int)MiicReadStatusSetting.UnRead).ToString()
                        }, out message4));
                    }
                }

                bool fileResult = false;
                try
                {
                    foreach (var item in publishAccessoryInfos)
                    {
                        string dest = HttpContext.Current.Server.MapPath(item.FilePath);
                        string source = string.Empty;
                        if (item.FileType == ((int)AccFileTypeSetting.Photo).ToString())
                        {
                            source = HttpContext.Current.Server.MapPath("/file/temp/PublishInfoAcc/Photo/" + Path.GetFileName(dest));
                        }
                        else
                        {
                            source = HttpContext.Current.Server.MapPath("/file/temp/PublishInfoAcc/File/" + Path.GetFileName(dest));
                        }
                        File.Copy(source, dest, true);
                    }

                    if (removeSimpleAccessoryViews != null && removeSimpleAccessoryViews.Count != 0)
                    {
                        foreach (var item in removeSimpleAccessoryViews)
                        {
                            File.Delete(HttpContext.Current.Server.MapPath(item.FilePath));
                            if (item.FileType == ((int)AccFileTypeSetting.Photo).ToString())
                            {
                                File.Delete(HttpContext.Current.Server.MapPath("/file/temp/PublishInfoAcc/Photo/" + Path.GetFileName(item.FilePath)));
                            }
                            else
                            {
                                File.Delete(HttpContext.Current.Server.MapPath("/file/temp/PublishInfoAcc/File/" + Path.GetFileName(item.FilePath)));
                            }
                        }
                    }

                    fileResult = true;
                }
                catch (Exception ex)
                {
                    Config.IlogicLogService.Write(new LogicLog()
                    {
                        AppName = Config.AppName,
                        ClassName = ClassName,
                        NamespaceName = NamespaceName,
                        MethodName = MethodBase.GetCurrentMethod().Name,
                        Message = ex.Message,
                        Oper = Config.Oper
                    });
                }

                if (fileResult == true)
                {
                    result = dbService.excuteSqls(sqls, out message);
                }
            }
            catch (Exception ex)
            {
                Config.IlogicLogService.Write(new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                });
            }
            if (result == true)
            {
                DeleteCache(true, o => o.ID == publishInfo.ID, o => o.PublishID == publishInfo.ID);
                InsertCaches(publishAccessoryInfos);
            }
            return result;
        }

        /// <summary>
        /// 搜索某人草稿列表
        /// </summary>
        /// <param name="keywordView">关键字视图</param>
        /// <param name="page">分页项</param>
        /// <returns>某人草稿列表</returns>
        public DataTable GetDraftInfos(DraftSearchView keywordView, MiicPage page)
        {
            Contract.Requires<ArgumentNullException>(keywordView != null, "参数keywordView:不能为空");
            DataTable result = new DataTable();
            MiicColumnCollections column = new MiicColumnCollections();
            List<MiicOrderBy> orders = new List<MiicOrderBy>();
            string message = string.Empty;
            MiicConditionCollections condition = keywordView.visitor(this);

            MiicOrderBy createTimeOrder = new MiicOrderBy()
            {
                Desc = true,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.CreateTime)
            };
            orders.Add(createTimeOrder);

            MiicOrderBy titleOrder = new MiicOrderBy()
            {
                Desc = true,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Title)
            };
            orders.Add(titleOrder);

            condition.order = orders;

            MiicFriendRelation accRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID),
              Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
              Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.PublishID),
              MiicDBOperatorSetting.Equal,
              MiicDBRelationSetting.LeftJoin);


            MiicFriendRelation userRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.CreaterID),
                Config.Attribute.GetSqlTableNameByClassName<Miic.Manage.User.SimplePersonUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.UserID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.LeftJoin);

            List<MiicFriendRelation> relations = new List<MiicFriendRelation>();
            relations.Add(accRelation);
            relations.Add(userRelation);

            MiicRelationCollections relation = new MiicRelationCollections(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(), relations);

            MiicColumn microPublishInfoAllColumns = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>());
            column.Add(microPublishInfoAllColumns);

            MiicColumn microAccessoryIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                string.Empty,
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID),
                "MomentsAccessoryInfoID");
            column.Add(microAccessoryIDColumn);
            MiicColumn fileNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileName));
            column.Add(fileNameColumn);
            MiicColumn filePathColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FilePath));
            column.Add(filePathColumn);
            MiicColumn uploadTime = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, DateTime?>(o => o.UploadTime));
            column.Add(uploadTime);
            MiicColumn fileTypeColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileType));
            column.Add(fileTypeColumn);

            MiicColumn orgNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<Miic.Manage.User.SimplePersonUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.OrgName));
            column.Add(orgNameColumn);
            MiicColumn userUrlColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<Miic.Manage.User.SimplePersonUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.UserUrl));
            column.Add(userUrlColumn);
            MiicColumn userTypeColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<Miic.Manage.User.SimplePersonUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.UserType));
            column.Add(userTypeColumn);
            try
            {
                if (page == null)
                {
                    result = dbService.GetInformations(column, relation, condition, out message);
                }
                else
                {
                    result = dbService.GetInformationsPage(column, relation, condition, page, out message, MiicDBPageRowNumberSetting.DenseRank);
                }
            }
            catch (Exception ex)
            {
                LogicLog log = new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                };
                Config.IlogicLogService.Write(log);
            }
            return result;
        }

        /// <summary>
        /// 获取某人草稿总数
        /// </summary>
        /// <param name="keywordView">关键字视图</param>
        /// <returns>个人草稿总数</returns>
        public int GetDraftInfoCount(DraftSearchView keywordView)
        {
            Contract.Requires<ArgumentNullException>(keywordView != null, "参数keywordView:不能为空");
            int result = 0;
            string message = string.Empty;
            MiicConditionCollections condition = keywordView.visitor(this);
            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID));
            try
            {
                result = dbService.GetCount<PublishInfo>(column, condition, out message, true);
            }
            catch (Exception ex)
            {
                LogicLog log = new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                };
                Config.IlogicLogService.Write(log);
            }
            return result;
        }

        /// <summary>
        /// 获取用户对于某朋友圈文章的行为状态
        /// </summary>
        /// <param name="behaviorView">用户行为视图</param>
        /// <returns>用户对于某朋友圈文章的行为状态（是否点赞、是否点踩、是否举报、是否收藏）</returns>
        DataTable IPublishInfo.GetMyMomentsBehaviorFlags(MyBehaviorView behaviorView)
        {
            Contract.Requires<ArgumentNullException>(behaviorView != null, "参数behaviorView：不能为空！");
            DataTable result = new DataTable();
            string message = string.Empty;
            string sql = behaviorView.visitor(this);
            try
            {
                result = dbService.querySql(sql, out message);
            }
            catch (Exception ex)
            {
                Config.IlogicLogService.Write(new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                });
            }
            return result;
        }
    }
}
