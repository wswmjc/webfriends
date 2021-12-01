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
    public class PraiseInfoDao : NoRelationCommon<PraiseInfo>, ICommunityBehavior<PraiseInfo>
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
        bool ICommon<PraiseInfo>.Insert(PraiseInfo praiseInfo)
        {
            Contract.Requires<ArgumentNullException>(praiseInfo != null, "参数praiseInfo：不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(praiseInfo.ID), "参数praiseInfo.ID：不能为空！");
            bool result = false;
            List<string> sqls = new List<string>();
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            string message3 = string.Empty;
            IPublishInfo Itemp = new PublishInfoDao();

            sqls.Add(DBService.InsertSql(praiseInfo, out message1));
            try
            {
                PublishInfo temp = ((ICommon<PublishInfo>)Itemp).GetInformation(praiseInfo.PublishID);
                sqls.Add(DBService.UpdateSql(new PublishInfo()
                {
                    ID = praiseInfo.PublishID,
                    PraiseNum = temp.PraiseNum + 1
                }, out message2));
                Notice.MessageInfo messageInfo = new Notice.MessageInfo()
                {
                    ID = Guid.NewGuid().ToString(),
                    PublisherID = praiseInfo.PraiserID,
                    PublisherName = praiseInfo.PraiserName,
                    PublishTime = praiseInfo.PraiseTime,
                    PublishID = praiseInfo.PublishID,
                    NoticerID = temp.CreaterID,
                    NoticerName = temp.CreaterName,
                    ReadStatus = ((int)Miic.Base.Setting.MiicReadStatusSetting.UnRead).ToString(),
                    Source = ((int)Miic.Friends.Common.Setting.BusinessTypeSetting.Community).ToString(),
                    MessageType = ((int)Miic.Friends.Behavior.Setting.BehaviorTypeSetting.Praise).ToString()
                };
                if (praiseInfo.PraiserID != temp.CreaterID)
                {
                    sqls.Add(DBService.InsertSql(messageInfo, out message3));
                }
                //积分
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = praiseInfo.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Behavior).ToString(),
                    Score = ScoreConfig.Score.BehaviorScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = praiseInfo.PraiserID,
                    UserName = praiseInfo.PraiserName
                }, out message));
                result = dbService.excuteSqls(sqls, out message);
                if (result == true)
                {
                    InsertCache(praiseInfo);
                    lock (syncRoot)
                    {
                        if (PublishInfoDao.items.Find(o => o.ID == praiseInfo.PublishID) != null)
                        {
                            PublishInfoDao.items.Find(o => o.ID == praiseInfo.PublishID).PraiseNum = temp.PraiseNum + 1;
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

        bool ICommon<PraiseInfo>.Update(PraiseInfo praiseInfo)
        {
            Contract.Requires<ArgumentNullException>(praiseInfo != null, "参数praiseInfo:不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(praiseInfo.ID), "参数praiseInfo.ID:不能为空，因为是主键");
            int count = 0;
            string message = string.Empty;
            bool result = false;
            try
            {
                result = dbService.Update(praiseInfo, out count, out message);
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
                DeleteCache(o => o.ID == praiseInfo.ID);
            }
            return result;
        }

        bool ICommon<PraiseInfo>.Delete(string id)
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
                PraiseInfo tempPraiseInfo = ((ICommon<PraiseInfo>)this).GetInformation(id);
                PublishInfo tempPublishInfo = ((ICommon<PublishInfo>)Itemp).GetInformation(tempPraiseInfo.PublishID);
                sqls.Add(DBService.DeleteSql(new PraiseInfo()
                {
                    ID = id
                }, out message1));
                sqls.Add(DBService.UpdateSql(new PublishInfo()
                {
                    ID = tempPraiseInfo.PublishID,
                    PraiseNum = tempPublishInfo.PraiseNum - 1
                }, out message2));
                Notice.MessageInfo messageInfo = new Notice.MessageInfo()
                {
                    ID = Guid.NewGuid().ToString(),
                    PublisherID = tempPraiseInfo.PraiserID,
                    PublisherName = tempPraiseInfo.PraiserName,
                    PublishTime = tempPraiseInfo.PraiseTime,
                    PublishID = tempPraiseInfo.PublishID,
                    NoticerID = tempPublishInfo.CreaterID,
                    NoticerName = tempPublishInfo.CreaterName,
                    ReadStatus = ((int)Miic.Base.Setting.MiicReadStatusSetting.UnRead).ToString(),
                    Source = ((int)Miic.Friends.Common.Setting.BusinessTypeSetting.Community).ToString(),
                    MessageType = ((int)Miic.Friends.Behavior.Setting.BehaviorTypeSetting.CancelPraise).ToString()
                };
                if (tempPraiseInfo.PraiserID != tempPublishInfo.CreaterID)
                {
                    sqls.Add(DBService.InsertSql(messageInfo, out message3));
                }
                //积分
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = tempPraiseInfo.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Behavior).ToString(),
                    Score = -1 * ScoreConfig.Score.BehaviorScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = tempPraiseInfo.PraiserID,
                    UserName = tempPraiseInfo.PraiserName
                }, out message));
                result = dbService.excuteSqls(sqls, out message);
                if (result == true)
                {
                    DeleteCache(o => o.ID == id);
                    lock (syncRoot)
                    {
                        if (PublishInfoDao.items.Find(o => o.ID == tempPraiseInfo.PublishID) != null)
                        {
                            PublishInfoDao.items.Find(o => o.ID == tempPraiseInfo.PublishID).PraiseNum = tempPublishInfo.PraiseNum - 1;
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

        PraiseInfo ICommon<PraiseInfo>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            PraiseInfo result = null;
            string message = string.Empty;
            try
            {
                result = items.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new PraiseInfo
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
                    result = Config.Serializer.Deserialize<PraiseInfo>(serializer);
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
        /// 查询当前登录人员对指定行业圈子信息的点赞信息
        /// </summary>
        /// <param name="communityBehaviorView">用户行为（点赞）视图</param>
        /// <returns>点赞信息</returns>
        public PraiseInfo GetUserPraiseInfo(MyCommunityBehaviorView communityBehaviorView)
        {
            Contract.Requires<ArgumentNullException>(communityBehaviorView != null, "参数communityBehaviorView:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(communityBehaviorView.PublishID), "参数communityBehaviorView.PublishID:不能为空");
            PraiseInfo result = null;
            string message = string.Empty;
            MiicConditionCollections condition = communityBehaviorView.visitor(this);
            MiicColumn praiseAll = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PraiseInfo>());
            MiicColumnCollections columns = new MiicColumnCollections();
            columns.Add(praiseAll);
            try
            {
                result = items.Find(o => o.PublishID == communityBehaviorView.PublishID && o.PraiserID == communityBehaviorView.LoginUserID);
                if (result == null)
                {
                    DataTable dt = dbService.GetInformations<PraiseInfo>(columns, condition, out message);
                    if (dt != null && dt.Rows.Count == 1)
                    {
                        result = new PraiseInfo()
                        {
                            ID = dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<PraiseInfo, string>(o => o.ID)].ToString(),
                            PraiserID = dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<PraiseInfo, string>(o => o.PraiserID)].ToString(),
                            PraiserName = dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<PraiseInfo, string>(o => o.PraiserName)].ToString(),
                            PraiseTime = (DateTime?)dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<PraiseInfo, DateTime?>(o => o.PraiseTime)],
                            PublishID = dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<PraiseInfo, string>(o => o.PublishID)].ToString(),
                            SortNo = (int?)dt.Rows[0][Config.Attribute.GetSqlColumnNameByPropertyName<PraiseInfo, int?>(o => o.SortNo)]
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
                    result = Config.Serializer.Deserialize<PraiseInfo>(serializer);
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
        /// 判断当前用户是否对制定行业圈子信息点赞
        /// </summary>
        /// <param name="communityBehaviorView"></param>
        /// <returns></returns>
        public bool IsPraise(MyCommunityBehaviorView communityBehaviorView)
        {
            Contract.Requires<ArgumentNullException>(communityBehaviorView != null, "参数communityBehaviorView:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(communityBehaviorView.PublishID), "参数communityBehaviorView.PublishID:不能为空");
            bool result = false;
            string message = string.Empty;
            MiicConditionCollections condition = communityBehaviorView.visitor(this);
            MiicColumn praiseID = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PraiseInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PraiseInfo, string>(o => o.ID));
            try
            {
                int count = dbService.GetCount<PraiseInfo>(praiseID, condition, out message);
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
