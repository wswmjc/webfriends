using Miic.Base;
using Miic.Base.Setting;
using Miic.Common;
using Miic.DB;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Common;
using Miic.Friends.Common.Setting;
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
    public class BrowseInfoDao : NoRelationCommon<BrowseInfo>, IBehavior<BrowseInfo>
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
        bool ICommon<BrowseInfo>.Insert(BrowseInfo browseInfo)
        {
            Contract.Requires<ArgumentNullException>(browseInfo != null, "参数browseInfo：不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(browseInfo.ID), "参数browseInfo.ID：不能为空！");
            bool result = false;
            List<string> sqls = new List<string>();
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            IPublishInfo Itemp = new PublishInfoDao();
            //设置提示
            browseInfo.IsHinted = ((int)MiicYesNoSetting.Yes).ToString();
            sqls.Add(DBService.InsertSql(browseInfo, out message1));
            //积分
            //sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
            //{
            //    ID = Guid.NewGuid().ToString(),
            //    BusinessID = browseInfo.PublishID,
            //    CreateTime = DateTime.Now,
            //    GetWay = ((int)GetWayTypeSetting.Behavior).ToString(),
            //    Score = ScoreConfig.BehaviorScore,
            //    ServiceID = ScoreConfig.ServiceID,
            //    UserID = browseInfo.BrowserID,
            //    UserName = browseInfo.BrowserName
            //}, out message));

            try
            {
                PublishInfo temp = ((ICommon<PublishInfo>)Itemp).GetInformation(browseInfo.PublishID);
                sqls.Add(DBService.UpdateSql(new PublishInfo()
                {
                    ID = browseInfo.PublishID,
                    BrowseNum = temp.BrowseNum + 1
                }, out message2));
                result = dbService.excuteSqls(sqls, out message);
                if (result == true)
                {
                    InsertCache(browseInfo);
                    lock (syncRoot)
                    {
                        if (PublishInfoDao.items.Find(o => o.ID == browseInfo.PublishID) != null)
                        {
                            PublishInfoDao.items.Find(o => o.ID == browseInfo.PublishID).BrowseNum = temp.BrowseNum + 1;
                        }
                    }
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

        bool ICommon<BrowseInfo>.Update(BrowseInfo browseInfo)
        {
            Contract.Requires<ArgumentNullException>(browseInfo != null, "参数browseInfo:不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(browseInfo.ID), "参数browseInfo.ID:不能为空，因为是主键");
            int count = 0;
            string message = string.Empty;
            bool result = false;
            try
            {
                result = dbService.Update(browseInfo, out count, out message);
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
                DeleteCache(o => o.ID == browseInfo.ID);
            }
            return result;
        }

        bool ICommon<BrowseInfo>.Delete(string id)
        {
            throw new NotSupportedException("浏览记录不支持删除");
        }

        BrowseInfo ICommon<BrowseInfo>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            BrowseInfo result = null;
            string message = string.Empty;
            try
            {
                result = items.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new BrowseInfo
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
                    result = Config.Serializer.Deserialize<BrowseInfo>(serializer);
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
        /// 获取浏览该朋友圈信息的列表
        /// </summary>
        /// <param name="publishID">朋友圈信息ID</param>
        /// <returns>浏览信息列表</returns>
        public DataTable GetBrowseInfosByPublishID(string publishID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(publishID), "参数publishID:不能为空");
            DataTable result = new DataTable();
            MiicCondition publishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<BrowseInfo, string>(o => o.PublishID),
               publishID,
               DbType.String,
               MiicDBOperatorSetting.Equal);
            string message = string.Empty;
            MiicColumn allColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<BrowseInfo>());
            MiicColumnCollections columns = new MiicColumnCollections();
            columns.Add(allColumn);
            MiicConditionSingle condition = new MiicConditionSingle(publishIDCondition);
            try
            {
                result = dbService.GetInformations<BrowseInfo>(columns, condition, out message);
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
        /// 获取浏览该朋友圈信息的数量
        /// </summary>
        /// <param name="publishID">朋友圈信息ID</param>
        /// <returns>浏览数</returns>
        public int? GetBrowseCountByPublishID(string publishID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(publishID), "参数publishID:不能为空");
            int result = 0;
            MiicCondition publishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<BrowseInfo, string>(o => o.PublishID),
               publishID,
               DbType.String,
               MiicDBOperatorSetting.Equal);
            string message = string.Empty;
            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<BrowseInfo>(),
                                               Config.Attribute.GetSqlColumnNameByPropertyName<BrowseInfo, string>(o => o.ID));
            MiicConditionSingle condition = new MiicConditionSingle(publishIDCondition);
            try
            {
                result = dbService.GetCount<BrowseInfo>(column, condition, out message);
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
        /// 获取最热朋友圈信息（条件：长篇、已发布的、上线的）
        /// </summary>
        /// <param name="top">top默认：15</param>
        /// <returns>最热朋友圈信息列表</returns>
        public DataTable GetTopHotestBrowseInfo(int top)
        {
            Contract.Requires<ArgumentOutOfRangeException>(top > 0, "参数top:不能为负");
            DataTable result = new DataTable();
            string message = string.Empty;
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition longInfoCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.PublishType),
               ((int)PublishInfoTypeSetting.Long).ToString(),
               DbType.String,
               MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, longInfoCondition));
            MiicCondition publishStatusCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.PublishTime),
               null,
               DbType.DateTime,
               MiicDBOperatorSetting.NotIsNull);
            condition.Add(new MiicConditionLeaf(publishStatusCondition));
            MiicCondition editStatusCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.EditStatus),
               ((int)MiicYesNoSetting.No).ToString(),
               DbType.String,
               MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(editStatusCondition));
            MiicOrderBy orderby = new MiicOrderBy()
            {
                Desc = true,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.BrowseNum)
            };
            List<MiicOrderBy> order = new List<MiicOrderBy>();
            order.Add(orderby);
            condition.order = order;

            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>());
            MiicColumnCollections columns = new MiicColumnCollections(new MiicTop(top));

            try
            {
                result = dbService.GetInformations<PublishInfo>(columns, condition, out message);
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
        /// 设置该浏览记录是否朋友圈信息搜索提醒
        /// </summary>
        /// <param name="yesNoView">Yes/No视图</param>
        /// <returns>Yes/No</returns>
        public bool SetHinted(YesNoView yesNoView)
        {
            Contract.Requires<ArgumentNullException>(yesNoView != null, "参数yearNoView:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            try
            {
                result = dbService.Update<BrowseInfo>(new BrowseInfo()
                {
                    ID = yesNoView.ID,
                    IsHinted = ((int)yesNoView.YesNo).ToString()
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
                DeleteCache(o => o.ID == yesNoView.ID);
            }
            return result;
        }

        /// <summary>
        ///  获取用户关注过的朋友圈信息列表提示
        /// </summary>
        /// <param name="keywordView">关键字视图</param>
        /// <param name="page">分页</param>
        /// <returns>朋友圈信息提示列表</returns>
        public DataTable GetHintedBrowseList(MyKeywordView keywordView, MiicPage page)
        {
            Contract.Requires<ArgumentNullException>(keywordView != null, "参数keywordView:不能为空");
            DataTable result = new DataTable();
            string message = string.Empty;
            MiicConditionCollections condition = keywordView.visitor(this);
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<BrowseInfo>(),
              Config.Attribute.GetSqlColumnNameByPropertyName<BrowseInfo, string>(o => o.PublishID),
              Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
              Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID),
              MiicDBOperatorSetting.Equal,
              MiicDBRelationSetting.LeftJoin);
            MiicColumnCollections columns = new MiicColumnCollections();
            MiicColumn browseIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<BrowseInfo>(),
                                              Config.Attribute.GetSqlColumnNameByPropertyName<BrowseInfo, string>(o => o.ID));
            columns.Add(browseIDColumn);
            MiicColumn browserIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<BrowseInfo>(),
                                               Config.Attribute.GetSqlColumnNameByPropertyName<BrowseInfo, string>(o => o.BrowserID));
            columns.Add(browserIDColumn);
            MiicColumn browserNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<BrowseInfo>(),
                                              Config.Attribute.GetSqlColumnNameByPropertyName<BrowseInfo, string>(o => o.BrowserName));
            columns.Add(browserNameColumn);
            MiicColumn publishIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<BrowseInfo>(),
                                               Config.Attribute.GetSqlColumnNameByPropertyName<BrowseInfo, string>(o => o.PublishID));
            columns.Add(publishIDColumn);
            MiicColumn microTitleColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
                                               Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Title));
            columns.Add(microTitleColumn);
            MiicColumn microContentColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
                                               Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Content));
            columns.Add(microContentColumn);
            MiicColumn microTypeColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
                                               Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.PublishType));
            columns.Add(microTypeColumn);
            try
            {
                if (page == null)
                {
                    DataTable dt = dbService.GetInformations(columns, relation, condition, out message);
                    DataView dtView = dt.DefaultView;
                    result = dtView.ToTable(true);
                }
                else
                {
                    DataTable dtWithPage = dbService.GetInformationsPage(columns, relation, condition, page, out message);
                    DataView dtWithPageView = dtWithPage.DefaultView;
                    result = dtWithPageView.ToTable(true);
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
        /// 获取用户关注过的朋友圈信息数目
        /// </summary>
        /// <param name="keywordView">关键字视图</param>
        /// <returns></returns>
        public int GetHintedBrowseCount(MyKeywordView keywordView)
        {
            Contract.Requires<ArgumentNullException>(keywordView != null, "参数keywordView:不能为空");
            int result = 0;
            string message = string.Empty;
            MiicConditionCollections condition = keywordView.visitor(this);
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<BrowseInfo>(),
              Config.Attribute.GetSqlColumnNameByPropertyName<BrowseInfo, string>(o => o.PublishID),
              Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
              Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID),
              MiicDBOperatorSetting.Equal,
              MiicDBRelationSetting.LeftJoin);
            MiicColumnCollections columns = new MiicColumnCollections();
            MiicColumn browseIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<BrowseInfo>(),
                                              Config.Attribute.GetSqlColumnNameByPropertyName<BrowseInfo, string>(o => o.ID));
            columns.Add(browseIDColumn);
            MiicColumn browserIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<BrowseInfo>(),
                                               Config.Attribute.GetSqlColumnNameByPropertyName<BrowseInfo, string>(o => o.BrowserID));
            columns.Add(browserIDColumn);
            MiicColumn browserNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<BrowseInfo>(),
                                              Config.Attribute.GetSqlColumnNameByPropertyName<BrowseInfo, string>(o => o.BrowserName));
            columns.Add(browserNameColumn);
            MiicColumn publishIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<BrowseInfo>(),
                                               Config.Attribute.GetSqlColumnNameByPropertyName<BrowseInfo, string>(o => o.PublishID));
            columns.Add(publishIDColumn);
            MiicColumn microTitleColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
                                               Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Title));
            columns.Add(microTitleColumn);
            MiicColumn microContentColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
                                               Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Content));
            columns.Add(microContentColumn);
            MiicColumn microTypeColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
                                               Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.PublishType));
            columns.Add(microTypeColumn);
            try
            {
                DataTable dt = dbService.GetInformations(columns, relation, condition, out message);
                DataView dtView = dt.DefaultView;
                result = (dtView.ToTable(true)).Rows.Count;
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
