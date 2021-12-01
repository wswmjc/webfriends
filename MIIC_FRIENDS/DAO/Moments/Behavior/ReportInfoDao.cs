using Miic.Base;
using Miic.Base.Setting;
using Miic.DB;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Common;
using Miic.Friends.Moments.Behavior;
using Miic.Log;
using Miic.Manage.User;
using Miic.Manage.User.Setting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Moments.Behavior
{
    public class ReportInfoDao : NoRelationCommon<ReportInfo>, IBehavior<ReportInfo>
    {
        private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
        private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
        private object syncRoot = new object();
        public static bool DeleteCacheByPublishID(string publishID)
        {
            bool result = false;
            if (items.Count > 0)
            {
                try
                {
                    items.RemoveAll(o => o.PublishID == publishID);
                    result = true;
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

            }
            else
            {
                result = true;
            }
            return result;
        }
        bool ICommon<ReportInfo>.Insert(ReportInfo reportInfo)
        {
            Contract.Requires<ArgumentNullException>(reportInfo != null, "参数reportInfo：不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(reportInfo.ID), "参数reportInfo.ID：不能为空！");
            bool result = false;
            string message = string.Empty;
            List<string> sqls = new List<string>();
            //默认未受理
            reportInfo.ReportStatus = ((int)MiicYesNoSetting.No).ToString();
            sqls.Add(DBService.InsertSql<ReportInfo>(reportInfo, out message));
            //积分
            sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
            {
                ID = Guid.NewGuid().ToString(),
                BusinessID = reportInfo.ID,
                CreateTime = DateTime.Now,
                GetWay = ((int)GetWayTypeSetting.Behavior).ToString(),
                Score = ScoreConfig.Score.BehaviorScore,
                ServiceID = ScoreConfig.ServiceID,
                UserID = reportInfo.ReporterID,
                UserName = reportInfo.ReporterName
            }, out message));

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
                InsertCache(reportInfo);
            }
            return result;
        }

        bool ICommon<ReportInfo>.Update(ReportInfo reportInfo)
        {
            Contract.Requires<ArgumentNullException>(reportInfo != null, "参数reportInfo:不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(reportInfo.ID), "参数reportInfo.ID:不能为空，因为是主键");
            int count = 0;
            string message = string.Empty;
            bool result = false;
            try
            {
                result = dbService.Update(reportInfo, out count, out message);
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
                DeleteCache(o => o.ID == reportInfo.ID);
            }
            return result;
        }

        bool ICommon<ReportInfo>.Delete(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            bool result = false;
            string message = string.Empty;
            List<string> sqls = new List<string>();
            ReportInfo temp = ((ICommon<ReportInfo>)this).GetInformation(id);
            sqls.Add(DBService.DeleteSql<ReportInfo>(new ReportInfo()
            {
                ID = id
            }, out message));

            //积分
            sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
            {
                ID = Guid.NewGuid().ToString(),
                BusinessID = temp.ID,
                CreateTime = DateTime.Now,
                GetWay = ((int)GetWayTypeSetting.Behavior).ToString(),
                Score = -1 * ScoreConfig.Score.BehaviorScore,
                ServiceID = ScoreConfig.ServiceID,
                UserID = temp.ReporterID,
                UserName = temp.ReporterName
            }, out message));
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
                DeleteCache(o => o.ID == id);
            }
            return result;
        }

        ReportInfo ICommon<ReportInfo>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            ReportInfo result = null;
            string message = string.Empty;
            try
            {
                result = items.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new ReportInfo
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
                    result = Config.Serializer.Deserialize<ReportInfo>(serializer);
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
        /// 查询当前登录人员对指定微博的举报信息
        /// </summary>
        /// <param name="momentsBehaviorView">用户行为（举报）视图</param>
        /// <returns>举报信息</returns>
        public ReportInfo GetUserReportInfo(MyBehaviorView momentsBehaviorView)
        {
            Contract.Requires<ArgumentNullException>(momentsBehaviorView != null, "参数momentsBehaviorView:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(momentsBehaviorView.PublishID), "参数momentsBehaviorView.PublishID:不能为空");
            ReportInfo result = null;
            string message = string.Empty;
            MiicConditionCollections condition = momentsBehaviorView.visitor(this);
            MiicColumn reportAll = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<ReportInfo>());
            MiicColumnCollections columns = new MiicColumnCollections();
            columns.Add(reportAll);
            try
            {
                result = items.Find(o => o.PublishID == momentsBehaviorView.PublishID && o.ReporterID == momentsBehaviorView.UserID);
                if (result == null)
                {
                    DataTable dt = dbService.GetInformations<ReportInfo>(columns, condition, out message);
                    if (dt != null && dt.Rows.Count == 1)
                    {
                        result = new ReportInfo()
                        {
                            ID = dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfo, string>(o => o.ID)].ToString(),
                            ReporterID = dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfo, string>(o => o.ReporterID)].ToString(),
                            ReporterName = dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfo, string>(o => o.ReporterName)].ToString(),
                            ReportTime = (DateTime?)dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfo, DateTime?>(o => o.ReportTime)],
                            PublishID = dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfo, string>(o => o.PublishID)].ToString(),
                            SortNo = (int?)dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfo, int?>(o => o.SortNo)]
                        };
                    }
                    else
                    {
                        result = null;
                    }

                    if (result != null)
                    {
                        InsertCache(result);
                    }
                }
                else
                {
                    string serializer = Config.Serializer.Serialize(result);
                    result = Config.Serializer.Deserialize<ReportInfo>(serializer);
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
        /// 判断当前用户是否对制定微博举报
        /// </summary>
        /// <param name="momentsBehaviorView"></param>
        /// <returns></returns>
        public bool IsReport(MyBehaviorView momentsBehaviorView)
        {
            Contract.Requires<ArgumentNullException>(momentsBehaviorView != null, "参数momentsBehaviorView:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(momentsBehaviorView.PublishID), "参数momentsBehaviorView.PublishID:不能为空");
            bool result = false;
            string message = string.Empty;
            MiicConditionCollections condition = momentsBehaviorView.visitor(this);
            MiicColumn reportID = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<ReportInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfo, string>(o => o.ID));
            try
            {
                int count = dbService.GetCount<ReportInfo>(reportID, condition, out message);
                if (count > 0)
                {
                    result = true;
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
        /// 举报查询（用于管理）
        /// </summary>
        /// <param name="keywordView">关键字视图</param>
        /// <param name="page">分页，默认不分页</param>
        /// <returns>举报列表</returns>
        //public DataTable Search(NoPersonKeywordView keywordView, MiicPage page)
        //{
        //    DataTable result = new DataTable();
        //    string message = string.Empty;
        //    MiicConditionCollections condition = keywordView.visitor(this);
        //    List<MiicOrderBy> order = new List<MiicOrderBy>();
        //    MiicOrderBy timesOrder = new MiicOrderBy()
        //    {
        //        PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfoOn, int?>(o => o.ReportTimes),
        //        Desc = true
        //    };
        //    order.Add(timesOrder);
        //    condition.order = order;

        //    MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<ReportInfoOn>(),
        //                                             Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfoOn, string>(o => o.ReportPublishID),
        //                                             Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
        //                                             Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID),
        //                                             MiicDBOperatorSetting.Equal,
        //                                             MiicDBRelationSetting.InnerJoin);
        //    MiicColumnCollections columns = new MiicColumnCollections();
        //    MiicColumn publishIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
        //                                             Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID));
        //    columns.Add(publishIDColumn);
        //    MiicColumn titleColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
        //                                             Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Title));
        //    columns.Add(titleColumn);
        //    MiicColumn contentColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
        //                                           Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Content));
        //    columns.Add(contentColumn);
        //    MiicColumn typeColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
        //                                           Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Type));
        //    columns.Add(typeColumn);
        //    MiicColumn hasAccColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
        //                                           Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.HasAcc));
        //    columns.Add(hasAccColumn);
        //    MiicColumn createTimeColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
        //                                          Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.CreateTime));
        //    columns.Add(createTimeColumn);
        //    MiicColumn reportTimesColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<ReportInfoOn>(),
        //                                           Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfoOn, int?>(o => o.ReportTimes));
        //    columns.Add(reportTimesColumn);
        //    try
        //    {
        //        if (page == null)
        //        {
        //            result = dbService.GetInformations(columns, relation, condition, out message);
        //        }
        //        else
        //        {
        //            result = dbService.GetInformationsPage(columns, relation, condition, page, out message);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Config.IlogicLogService.Write(new LogicLog()
        //        {
        //            AppName = Config.AppName,
        //            ClassName = ClassName,
        //            NamespaceName = NamespaceName,
        //            MethodName = MethodBase.GetCurrentMethod().Name,
        //            Message = ex.Message,
        //            Oper = Config.Oper
        //        });
        //    }
        //    return result;
        //}
        /// <summary>
        /// 举报查询数（用于管理）
        /// </summary>
        /// <param name="keywordView">关键字视图</param>
        /// <returns>举报列表数</returns>
        //public int GetSearchCount(NoPersonKeywordView keywordView)
        //{
        //    int result = 0;
        //    string message = string.Empty;
        //    MiicConditionCollections condition = keywordView.visitor(this);
        //    MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<ReportInfoOn>(),
        //                                             Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfoOn, string>(o => o.ReportPublishID),
        //                                             Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
        //                                             Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID),
        //                                             MiicDBOperatorSetting.Equal,
        //                                             MiicDBRelationSetting.InnerJoin);
        //    MiicColumn publishIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
        //                                             Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID));
        //    try
        //    {
        //        result = dbService.GetCount(publishIDColumn, relation, condition, out message);
        //    }
        //    catch (Exception ex)
        //    {
        //        Config.IlogicLogService.Write(new LogicLog()
        //        {
        //            AppName = Config.AppName,
        //            ClassName = ClassName,
        //            NamespaceName = NamespaceName,
        //            MethodName = MethodBase.GetCurrentMethod().Name,
        //            Message = ex.Message,
        //            Oper = Config.Oper
        //        });
        //    }
        //    return result;
        //}

        /// <summary>
        /// 受理举报
        /// </summary>
        /// <param name="publishID">被举报微博ID</param>
        /// <returns>true/false</returns>
        public bool HandleReport(string publishID)
        {
            string message = string.Empty;
            string message1 = string.Empty;
            int count = 0;
            bool result = false;
            MiicCondition publishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfo, string>(o => o.PublishID),
                publishID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            MiicConditionSingle condition = new MiicConditionSingle(publishIDCondition);
            string sql = DBService.UpdateConditionSql<ReportInfo>(new ReportInfo
            {
                ReportStatus = ((int)MiicYesNoSetting.Yes).ToString()
            }, condition, out message);
            try
            {
                result = dbService.excuteSql(sql, out count, out message1);
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
