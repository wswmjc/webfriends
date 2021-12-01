using Miic.Base;
using Miic.Base.Setting;
using Miic.DB;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Common;
using Miic.Friends.Community.Behavior;
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

namespace Miic.Friends.Community.Behavior
{
    public class CollectInfoDao : NoRelationCommon<CollectInfo>, ICommunityBehavior<CollectInfo>
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
        bool ICommon<CollectInfo>.Insert(CollectInfo collectInfo)
        {
            Contract.Requires<ArgumentNullException>(collectInfo != null, "参数collectInfo：不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(collectInfo.ID), "参数collectInfo.ID：不能为空！");
            bool result = false;
            List<string> sqls = new List<string>();
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            string message3 = string.Empty;
            IPublishInfo Itemp = new PublishInfoDao();
            sqls.Add(DBService.InsertSql(collectInfo, out message1));
            try
            {
                PublishInfo temp = ((ICommon<PublishInfo>)Itemp).GetInformation(collectInfo.PublishID);
                sqls.Add(DBService.UpdateSql(new PublishInfo()
                {
                    ID = collectInfo.PublishID,
                    CollectNum = temp.CollectNum + 1
                }, out message2));
                Notice.MessageInfo messageInfo = new Notice.MessageInfo()
                {
                    ID = Guid.NewGuid().ToString(),
                    PublisherID = collectInfo.CollectorID,
                    PublisherName = collectInfo.CollectorName,
                    PublishTime = collectInfo.CollectTime,
                    PublishID = collectInfo.PublishID,
                    NoticerID = temp.CreaterID,
                    NoticerName = temp.CreaterName,
                    ReadStatus = ((int)MiicReadStatusSetting.UnRead).ToString(),
                    Source = ((int)Miic.Friends.Common.Setting.BusinessTypeSetting.Community).ToString(),
                    MessageType = ((int)Miic.Friends.Behavior.Setting.BehaviorTypeSetting.Collect).ToString()
                };
                if (collectInfo.CollectorID != temp.CreaterID)
                {
                    sqls.Add(DBService.InsertSql(messageInfo, out message3));
                }
                result = dbService.excuteSqls(sqls, out message);
                //积分
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = collectInfo.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Behavior).ToString(),
                    Score = ScoreConfig.Score.BehaviorScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = collectInfo.CollectorID,
                    UserName = collectInfo.CollectorName
                }, out message));
                if (result == true)
                {
                    InsertCache(collectInfo);
                    lock (syncRoot)
                    {
                        if (PublishInfoDao.items.Find(o => o.ID == collectInfo.PublishID) != null)
                        {
                            PublishInfoDao.items.Find(o => o.ID == collectInfo.PublishID).CollectNum = temp.CollectNum + 1;
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

        bool ICommon<CollectInfo>.Update(CollectInfo collectInfo)
        {
            Contract.Requires<ArgumentNullException>(collectInfo != null, "参数collectInfo:不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(collectInfo.ID), "参数collectInfo.ID:不能为空，因为是主键");
            int count = 0;
            string message = string.Empty;
            bool result = false;
            try
            {
                result = dbService.Update(collectInfo, out count, out message);
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
                DeleteCache(o => o.ID == collectInfo.ID);
            }
            return result;
        }

        bool ICommon<CollectInfo>.Delete(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            bool result = false;
            List<string> sqls = new List<string>();
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            string message3 = string.Empty;
            IPublishInfo Itemp = new PublishInfoDao();
            try
            {
                CollectInfo tempCollectInfo = ((ICommon<CollectInfo>)this).GetInformation(id);
                PublishInfo tempPublishInfo = ((ICommon<PublishInfo>)Itemp).GetInformation(tempCollectInfo.PublishID);
                sqls.Add(DBService.DeleteSql(new CollectInfo()
                {
                    ID = id
                }, out message1));
                sqls.Add(DBService.UpdateSql(new PublishInfo()
                {
                    ID = tempCollectInfo.PublishID,
                    CollectNum = tempPublishInfo.CollectNum - 1
                }, out message2));
                Notice.MessageInfo messageInfo = new Notice.MessageInfo()
                {
                    ID = Guid.NewGuid().ToString(),
                    PublisherID = tempCollectInfo.CollectorID,
                    PublisherName = tempCollectInfo.CollectorName,
                    PublishTime = tempCollectInfo.CollectTime,
                    PublishID = tempCollectInfo.PublishID,
                    NoticerID = tempPublishInfo.CreaterID,
                    NoticerName = tempPublishInfo.CreaterName,
                    ReadStatus = ((int)MiicReadStatusSetting.UnRead).ToString(),
                    Source = ((int)Miic.Friends.Common.Setting.BusinessTypeSetting.Community).ToString(),
                    MessageType = ((int)Miic.Friends.Behavior.Setting.BehaviorTypeSetting.CancelCollect).ToString()
                };
                if (tempCollectInfo.CollectorID != tempPublishInfo.CreaterID)
                {
                    sqls.Add(DBService.InsertSql(messageInfo, out message3));
                }

                //积分
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = tempCollectInfo.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Behavior).ToString(),
                    Score = -1 * ScoreConfig.Score.BehaviorScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = tempCollectInfo.CollectorID,
                    UserName = tempCollectInfo.CollectorName
                }, out message));

                result = dbService.excuteSqls(sqls, out message);
                if (result == true)
                {
                    DeleteCache(o => o.ID == id);
                    lock (syncRoot)
                    {
                        if (PublishInfoDao.items.Find(o => o.ID == tempCollectInfo.PublishID) != null)
                        {
                            PublishInfoDao.items.Find(o => o.ID == tempCollectInfo.PublishID).CollectNum = tempPublishInfo.CollectNum - 1;
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

        CollectInfo ICommon<CollectInfo>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            CollectInfo result = null;
            string message = string.Empty;
            try
            {
                result = items.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new CollectInfo
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
                    result = Config.Serializer.Deserialize<CollectInfo>(serializer);
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
        /// 我的收藏信息列表
        /// </summary>
        /// <param name="keywordView">关键字视图</param>
        /// <param name="orderView">排序视图</param>
        /// <param name="page">分页</param>
        /// <returns>收藏信息列表</returns>
        public DataTable GetCollectInfos(MyKeywordView keywordView, MiicPage page)
        {
            Contract.Requires<ArgumentNullException>(keywordView != null, "参数keywordView:不能为空");
            DataTable result = new DataTable();
            string message = string.Empty;
            List<MiicOrderBy> orders = new List<MiicOrderBy>();
            MiicColumnCollections column = new MiicColumnCollections();
            MiicConditionCollections condition = keywordView.visitor(this);

            MiicOrderBy collectOrder = new MiicOrderBy()
            {
                Desc = true,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, DateTime?>(o => o.CollectTime)
            };
            orders.Add(collectOrder);

            condition.order = orders;

            MiicFriendRelation collectRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID),
              Config.Attribute.GetSqlTableNameByClassName<CollectInfo>(),
              Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.PublishID),
              MiicDBOperatorSetting.Equal,
              MiicDBRelationSetting.InnerJoin);

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
            relations.Add(collectRelation);
            relations.Add(accRelation);
            relations.Add(userRelation);

            MiicRelationCollections relation = new MiicRelationCollections(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(), relations);


            MiicColumn microPartakePublishInfoAllColumns = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>());
            column.Add(microPartakePublishInfoAllColumns);

            MiicColumn microCollectInfoIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CollectInfo>(),
          string.Empty,
          Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.ID),
          "CommunityCollectInfoID");
            column.Add(microCollectInfoIDColumn);
            MiicColumn collectTime = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CollectInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, DateTime?>(o => o.CollectTime));
            column.Add(collectTime);
            //收藏人员表
            MiicColumn collectorIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CollectInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.CollectorID));
            column.Add(collectorIDColumn);
            MiicColumn collectorNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CollectInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.CollectorName));
            column.Add(collectorNameColumn);

            MiicColumn microAccessoryIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                string.Empty,
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID),
                "CommunityAccessoryInfoID");
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
        /// 我的收藏信息数
        /// </summary>
        /// <param name="keywordView">关键字视图</param>
        /// <returns>收藏信息总数</returns>
        public int GetCollectCount(MyKeywordView keywordView)
        {
            Contract.Requires<ArgumentNullException>(keywordView != null, "参数keywordView:不能为空");
            int result = 0;
            string message = string.Empty;
            MiicConditionCollections condition = keywordView.visitor(this);
            MiicFriendRelation collectRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID),
              Config.Attribute.GetSqlTableNameByClassName<CollectInfo>(),
              Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.PublishID),
              MiicDBOperatorSetting.Equal,
              MiicDBRelationSetting.InnerJoin);

            MiicFriendRelation accRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID),
              Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
              Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.PublishID),
              MiicDBOperatorSetting.Equal,
              MiicDBRelationSetting.LeftJoin);

            List<MiicFriendRelation> relations = new List<MiicFriendRelation>();
            relations.Add(collectRelation);
            relations.Add(accRelation);

            MiicRelationCollections relation = new MiicRelationCollections(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(), relations);

            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CollectInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.ID));
            try
            {
                result = dbService.GetCount(column, relation, condition, out message);
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
        /// 查询当前登录人员对指定行业圈子信息的收藏信息
        /// </summary>
        /// <param name="communityBehaviorView">用户行为（收藏）视图</param>
        /// <returns>收藏信息</returns>
        public CollectInfo GetUserCollectInfo(MyCommunityBehaviorView communityBehaviorView)
        {
            Contract.Requires<ArgumentNullException>(communityBehaviorView != null, "参数communityBehaviorView:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(communityBehaviorView.PublishID), "参数communityBehaviorView.PublishID:不能为空");
            CollectInfo result = null;
            string message = string.Empty;
            MiicConditionCollections condition = communityBehaviorView.visitor(this);
            MiicColumnCollections columns = new MiicColumnCollections();
            MiicColumn collectAll = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CollectInfo>());
            columns.Add(collectAll);
            try
            {
                result = items.Find(o => o.PublishID == communityBehaviorView.PublishID && o.CollectorID == communityBehaviorView.LoginUserID);
                if (result == null)
                {
                    DataTable dt = dbService.GetInformations<CollectInfo>(columns, condition, out message);
                    if (dt != null && dt.Rows.Count == 1)
                    {
                        result = new CollectInfo()
                        {
                            ID = dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.ID)].ToString(),
                            CollectorID = dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.CollectorID)].ToString(),
                            CollectorName = dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.CollectorName)].ToString(),
                            CollectTime = (DateTime?)dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, DateTime?>(o => o.CollectTime)],
                            PublishID = dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.PublishID)].ToString(),
                            SortNo = (int?)dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, int?>(o => o.SortNo)]
                        };
                    }
                    if (result != null)
                    {
                        InsertCache(result);
                    }
                }
                else
                {
                    string serializer = Config.Serializer.Serialize(result);
                    result = Config.Serializer.Deserialize<CollectInfo>(serializer);
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
        /// 判断当前用户是否对制定行业圈子信息收藏
        /// </summary>
        /// <param name="communityBehaviorView"></param>
        /// <returns></returns>
        public bool IsCollect(MyCommunityBehaviorView communityBehaviorView)
        {
            Contract.Requires<ArgumentNullException>(communityBehaviorView != null, "参数communityBehaviorView:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(communityBehaviorView.PublishID), "参数communityBehaviorView.PublishID:不能为空");
            bool result = false;
            string message = string.Empty;
            MiicConditionCollections condition = communityBehaviorView.visitor(this);
            MiicColumn collectID = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CollectInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.ID));
            try
            {
                int count = dbService.GetCount<CollectInfo>(collectID, condition, out message);
                if (count == 0)
                {
                    result = false;
                }
                else
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


        public bool RecoverCollect(string publishID)
        {
            Contract.Requires<ArgumentNullException>(publishID != null, "参数publishID:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(publishID), "参数publishID:不能为空");
            bool result = false;
            string message = string.Empty;
            int count = 0;

            MiicCondition collectPublishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.PublishID),
               publishID,
               DbType.String,
               MiicDBOperatorSetting.Equal);
            MiicConditionSingle collectCondition = new MiicConditionSingle(collectPublishIDCondition);

            try
            {
                result = dbService.UpdateCondition<CollectInfo>(new CollectInfo()
                {
                    CollectValid = ((int)MiicValidTypeSetting.Valid).ToString()
                }, collectCondition, out count, out message);
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
                items.RemoveAll(o => o.PublishID == publishID);
            }
            return result;
        }
    }
}
