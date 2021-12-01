using Miic.Base;
using Miic.DB;
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
    public class TreadInfoDao : NoRelationCommon<TreadInfo>, ICommunityBehavior<TreadInfo>
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
        bool ICommon<TreadInfo>.Insert(TreadInfo treadInfo)
        {
            Contract.Requires<ArgumentNullException>(treadInfo != null, "参数treadInfo：不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(treadInfo.ID), "参数treadInfo.ID：不能为空！");
            bool result = false;
            List<string> sqls = new List<string>();
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            string message3 = string.Empty;
            IPublishInfo Itemp = new PublishInfoDao();
            sqls.Add(DBService.InsertSql(treadInfo, out message1));
            try
            {
                PublishInfo temp = ((ICommon<PublishInfo>)Itemp).GetInformation(treadInfo.PublishID);
                sqls.Add(DBService.UpdateSql(new PublishInfo()
                {
                    ID = treadInfo.PublishID,
                    TreadNum = temp.TreadNum + 1
                }, out message2));
                Notice.MessageInfo messageInfo = new Notice.MessageInfo()
                {
                    ID = Guid.NewGuid().ToString(),
                    PublisherID = treadInfo.TreaderID,
                    PublisherName = treadInfo.TreaderName,
                    PublishTime = treadInfo.TreadTime,
                    PublishID = treadInfo.PublishID,
                    NoticerID = temp.CreaterID,
                    NoticerName = temp.CreaterName,
                    ReadStatus = ((int)Miic.Base.Setting.MiicReadStatusSetting.UnRead).ToString(),
                    Source = ((int)Miic.Friends.Common.Setting.BusinessTypeSetting.Community).ToString(),
                    MessageType = ((int)Miic.Friends.Behavior.Setting.BehaviorTypeSetting.Tread).ToString()
                };
                if (treadInfo.TreaderID != temp.CreaterID)
                {
                    sqls.Add(DBService.InsertSql(messageInfo, out message3));
                }
                //积分
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = treadInfo.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Behavior).ToString(),
                    Score = ScoreConfig.Score.BehaviorScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = treadInfo.TreaderID,
                    UserName = treadInfo.TreaderName
                }, out message));
                result = dbService.excuteSqls(sqls, out message);
                if (result == true)
                {
                    InsertCache(treadInfo);
                    lock (syncRoot)
                    {
                        if (PublishInfoDao.items.Find(o => o.ID == treadInfo.PublishID) != null)
                        {
                            PublishInfoDao.items.Find(o => o.ID == treadInfo.PublishID).TreadNum = temp.TreadNum + 1;
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

        bool ICommon<TreadInfo>.Update(TreadInfo treadInfo)
        {
            Contract.Requires<ArgumentNullException>(treadInfo != null, "参数treadInfo:不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(treadInfo.ID), "参数treadInfo.ID:不能为空，因为是主键");
            int count = 0;
            string message = string.Empty;
            bool result = false;
            try
            {
                result = dbService.Update(treadInfo, out count, out message);
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
                DeleteCache(o => o.ID == treadInfo.ID);
            }
            return result;
        }

        bool ICommon<TreadInfo>.Delete(string id)
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
                TreadInfo tempThreadInfo = ((ICommon<TreadInfo>)this).GetInformation(id);
                PublishInfo tempPublishInfo = ((ICommon<PublishInfo>)Itemp).GetInformation(tempThreadInfo.PublishID);
                sqls.Add(DBService.DeleteSql(new TreadInfo()
                {
                    ID = id
                }, out message1));
                sqls.Add(DBService.UpdateSql(new PublishInfo()
                {
                    ID = tempThreadInfo.PublishID,
                    TreadNum = tempPublishInfo.TreadNum - 1
                }, out message2));
                Notice.MessageInfo messageInfo = new Notice.MessageInfo()
                {
                    ID = Guid.NewGuid().ToString(),
                    PublisherID = tempThreadInfo.TreaderID,
                    PublisherName = tempThreadInfo.TreaderName,
                    PublishTime = tempThreadInfo.TreadTime,
                    PublishID = tempThreadInfo.PublishID,
                    NoticerID = tempPublishInfo.CreaterID,
                    NoticerName = tempPublishInfo.CreaterName,
                    ReadStatus = ((int)Miic.Base.Setting.MiicReadStatusSetting.UnRead).ToString(),
                    Source = ((int)Miic.Friends.Common.Setting.BusinessTypeSetting.Community).ToString(),
                    MessageType = ((int)Miic.Friends.Behavior.Setting.BehaviorTypeSetting.CancelTread).ToString()
                };
                if (tempThreadInfo.TreaderID != tempPublishInfo.CreaterID)
                {
                    sqls.Add(DBService.InsertSql(messageInfo, out message3));
                }
                //积分
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = tempThreadInfo.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Behavior).ToString(),
                    Score = -1 * ScoreConfig.Score.BehaviorScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = tempThreadInfo.TreaderID,
                    UserName = tempThreadInfo.TreaderName
                }, out message));
                result = dbService.excuteSqls(sqls, out message);
                if (result == true)
                {
                    DeleteCache(o => o.ID == id);
                    lock (syncRoot)
                    {
                        if (PublishInfoDao.items.Find(o => o.ID == tempThreadInfo.PublishID) != null)
                        {
                            PublishInfoDao.items.Find(o => o.ID == tempThreadInfo.PublishID).TreadNum = tempPublishInfo.TreadNum - 1;
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

        TreadInfo ICommon<TreadInfo>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            TreadInfo result = null;
            string message = string.Empty;
            try
            {
                result = items.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new TreadInfo
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
                    result = Config.Serializer.Deserialize<TreadInfo>(serializer);
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
        /// 查询当前登录人员对指定行业圈子信息的点踩信息
        /// </summary>
        /// <param name="communityBehaviorView">用户行为（点踩）视图</param>
        /// <returns>点踩信息</returns>
        public TreadInfo GetUserTreadInfo(MyCommunityBehaviorView communityBehaviorView)
        {
            Contract.Requires<ArgumentNullException>(communityBehaviorView != null, "参数communityBehaviorView:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(communityBehaviorView.PublishID), "参数communityBehaviorView.PublishID:不能为空");
            TreadInfo result = null;
            string message = string.Empty;
            MiicConditionCollections condition = communityBehaviorView.visitor(this);
            MiicColumn threadAll = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<TreadInfo>());
            MiicColumnCollections columns = new MiicColumnCollections();
            columns.Add(threadAll);
            try
            {
                result = items.Find(o => o.PublishID == communityBehaviorView.PublishID && o.TreaderID == communityBehaviorView.LoginUserID);
                if (result == null)
                {
                    DataTable dt = dbService.GetInformations<TreadInfo>(columns, condition, out message);
                    if (dt != null && dt.Rows.Count == 1)
                    {
                        result = new TreadInfo()
                        {
                            ID = dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<TreadInfo, string>(o => o.ID)].ToString(),
                            TreaderID = dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<TreadInfo, string>(o => o.TreaderID)].ToString(),
                            TreaderName = dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<TreadInfo, string>(o => o.TreaderName)].ToString(),
                            TreadTime = (DateTime?)dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<TreadInfo, DateTime?>(o => o.TreadTime)],
                            PublishID = dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<TreadInfo, string>(o => o.PublishID)].ToString(),
                            SortNo = (int?)dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<TreadInfo, int?>(o => o.SortNo)]
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
                    result = Config.Serializer.Deserialize<TreadInfo>(serializer);
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
        /// 判断当前用户是否对制定行业圈子信息点过踩
        /// </summary>
        /// <param name="communityBehaviorView"></param>
        /// <returns></returns>
        public bool IsTread(MyCommunityBehaviorView communityBehaviorView)
        {
            Contract.Requires<ArgumentNullException>(communityBehaviorView != null, "参数communityBehaviorView:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(communityBehaviorView.PublishID), "参数communityBehaviorView.PublishID:不能为空");
            bool result = false;
            string message = string.Empty;
            MiicConditionCollections condition = communityBehaviorView.visitor(this);
            MiicColumn threadID = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<TreadInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<TreadInfo, string>(o => o.ID));
            try
            {
                int count = dbService.GetCount<TreadInfo>(threadID, condition, out message);
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
    }
}
