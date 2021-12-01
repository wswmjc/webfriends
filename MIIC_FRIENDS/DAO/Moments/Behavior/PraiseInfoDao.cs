using Miic.Base;
using Miic.Base.Setting;
using Miic.DB;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Behavior.Setting;
using Miic.Friends.Common;
using Miic.Friends.Common.Setting;
using Miic.Friends.Moments.Behavior;
using Miic.Friends.Notice;
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
    public class PraiseInfoDao : NoRelationCommon<PraiseInfo>, IBehavior<PraiseInfo>
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
            IPublishInfo IpublishInfo = new PublishInfoDao();
            sqls.Add(DBService.InsertSql(praiseInfo, out message1));
            PublishInfo temp = ((ICommon<PublishInfo>)IpublishInfo).GetInformation(praiseInfo.PublishID);
            if (temp == null)
            {
                throw new NoNullAllowedException("temp发布信息不能为空,请核查！");
            }
            sqls.Add(DBService.UpdateSql(new PublishInfo()
            {
                ID = praiseInfo.PublishID,
                PraiseNum = temp.PraiseNum + 1
            }, out message2));
            Miic.Friends.Notice.MessageInfo messageInfo = new Miic.Friends.Notice.MessageInfo()
            {
                ID = Guid.NewGuid().ToString(),
                PublisherID = praiseInfo.PraiserID,
                PublisherName = praiseInfo.PraiserName,
                PublishTime = praiseInfo.PraiseTime,
                PublishID = praiseInfo.PublishID,
                NoticerID = temp.CreaterID,
                NoticerName = temp.CreaterName,
                ReadStatus = ((int)MiicReadStatusSetting.UnRead).ToString(),
                Source = ((int)BusinessTypeSetting.Moments).ToString(),
                MessageType = ((int)BehaviorTypeSetting.Praise).ToString()
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
                InsertCache(praiseInfo);
                if (praiseInfo.PraiserID != temp.CreaterID)
                {
                    Miic.Friends.Notice.MessageInfoDao.InsertCache(messageInfo);
                }
                lock (syncRoot)
                {
                    if (PublishInfoDao.items.Find(o => o.ID == praiseInfo.PublishID) != null)
                    {
                        PublishInfoDao.items.Find(o => o.ID == praiseInfo.PublishID).PraiseNum = temp.PraiseNum + 1;
                    }
                }
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
            string message4 = string.Empty;
            PraiseInfo tempPraiseInfo = ((ICommon<PraiseInfo>)this).GetInformation(id);
            if (tempPraiseInfo == null)
            {
                throw new NoNullAllowedException("点赞信息不能为空，请核查！");
            }
            IPublishInfo IpublishInfo = new PublishInfoDao();
            PublishInfo tempPublishInfo = ((ICommon<PublishInfo>)IpublishInfo).GetInformation(tempPraiseInfo.PublishID);
            if (tempPublishInfo == null)
            {
                throw new NoNullAllowedException("朋友圈发布信息不能为空，请核查！");
            }
            sqls.Add(DBService.DeleteSql(new PraiseInfo()
            {
                ID = id
            }, out message1));
            sqls.Add(DBService.UpdateSql(new PublishInfo()
            {
                ID = tempPraiseInfo.PublishID,
                PraiseNum = tempPublishInfo.PraiseNum - 1
            }, out message2));
            Miic.Friends.Notice.MessageInfo messageInfo = new Miic.Friends.Notice.MessageInfo()
            {
                ID = Guid.NewGuid().ToString(),
                PublisherID = tempPraiseInfo.PraiserID,
                PublisherName = tempPraiseInfo.PraiserName,
                PublishTime = tempPraiseInfo.PraiseTime,
                PublishID = tempPraiseInfo.PublishID,
                NoticerID = tempPublishInfo.CreaterID,
                NoticerName = tempPublishInfo.CreaterName,
                ReadStatus = ((int)MiicReadStatusSetting.UnRead).ToString(),
                Source = ((int)BusinessTypeSetting.Moments).ToString(),
                MessageType = ((int)BehaviorTypeSetting.CancelPraise).ToString()
            };
            if (tempPraiseInfo.PraiserID != tempPublishInfo.CreaterID)
            {
                sqls.Add(DBService.InsertSql(messageInfo, out message4));
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
                if (tempPraiseInfo.PraiserID != tempPublishInfo.CreaterID)
                {
                    Miic.Friends.Notice.MessageInfoDao.InsertCache(messageInfo);
                }
                DeleteCache(o => o.ID == id);
                lock (syncRoot)
                {
                    if (PublishInfoDao.items.Find(o => o.ID == tempPraiseInfo.PublishID) != null)
                    {
                        PublishInfoDao.items.Find(o => o.ID == tempPraiseInfo.PublishID).PraiseNum = tempPublishInfo.PraiseNum - 1;
                    }
                }
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
        /// 查询当前登录人员对指定朋友圈信息的点赞信息
        /// </summary>
        /// <param name="momentsBehaviorView">用户行为（点赞）视图</param>
        /// <returns>点赞信息</returns>
        public PraiseInfo GetUserPraiseInfo(MyBehaviorView momentsBehaviorView)
        {
            Contract.Requires<ArgumentNullException>(momentsBehaviorView != null, "参数momentsBehaviorView:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(momentsBehaviorView.PublishID), "参数momentsBehaviorView.PublishID:不能为空");
            PraiseInfo result = null;
            string message = string.Empty;
            MiicConditionCollections condition = momentsBehaviorView.visitor(this);
            MiicColumn praiseAll = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PraiseInfo>());
            MiicColumnCollections columns = new MiicColumnCollections();
            columns.Add(praiseAll);
            try
            {
                result = items.Find(o => o.PublishID == momentsBehaviorView.PublishID && o.PraiserID == momentsBehaviorView.UserID);
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
        /// 判断当前用户是否对制定朋友圈信息点赞
        /// </summary>
        /// <param name="momentsBehaviorView"></param>
        /// <returns></returns>
        public bool IsPraise(MyBehaviorView momentsBehaviorView)
        {
            Contract.Requires<ArgumentNullException>(momentsBehaviorView != null, "参数momentsBehaviorView:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(momentsBehaviorView.PublishID), "参数momentsBehaviorView.PublishID:不能为空");
            bool result = false;
            string message = string.Empty;
            MiicConditionCollections condition = momentsBehaviorView.visitor(this);
            MiicColumn praiseID = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PraiseInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PraiseInfo, string>(o => o.ID));
            try
            {
                int count = dbService.GetCount<PraiseInfo>(praiseID, condition, out message);
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
    }
}
