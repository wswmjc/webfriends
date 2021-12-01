using Miic.Base;
using Miic.Base.Setting;
using Miic.DB;
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
    public class TreadInfoDao : NoRelationCommon<TreadInfo>, IBehavior<TreadInfo>
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
            IPublishInfo IpublishInfo = new PublishInfoDao();
            PublishInfo temp = ((ICommon<PublishInfo>)IpublishInfo).GetInformation(treadInfo.PublishID);
            if (temp == null)
            {
                throw new NoNullAllowedException("temp发布信息不能为空,请核查！");
            }
            sqls.Add(DBService.InsertSql(treadInfo, out message1));
            sqls.Add(DBService.UpdateSql(new PublishInfo()
            {
                ID = treadInfo.PublishID,
                TreadNum = temp.TreadNum + 1
            }, out message2));
            Miic.Friends.Notice.MessageInfo messageInfo = new Miic.Friends.Notice.MessageInfo()
            {
                ID = Guid.NewGuid().ToString(),
                PublisherID = treadInfo.TreaderID,
                PublisherName = treadInfo.TreaderName,
                PublishTime = treadInfo.TreadTime,
                PublishID = treadInfo.PublishID,
                NoticerID = temp.CreaterID,
                NoticerName = temp.CreaterName,
                ReadStatus = ((int)MiicReadStatusSetting.UnRead).ToString(),
                Source = ((int)BusinessTypeSetting.Moments).ToString(),
                MessageType = ((int)BehaviorTypeSetting.Tread).ToString()
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
                InsertCache(treadInfo);
                lock (syncRoot)
                {
                    if (PublishInfoDao.items.Find(o => o.ID == treadInfo.PublishID) != null)
                    {
                        PublishInfoDao.items.Find(o => o.ID == treadInfo.PublishID).TreadNum = temp.TreadNum + 1;
                    }
                }
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
            IPublishInfo IpublishInfo = new PublishInfoDao();
            TreadInfo tempTreadInfo = ((ICommon<TreadInfo>)this).GetInformation(id);
            if (tempTreadInfo == null)
            {
                throw new NoNullAllowedException("点踩信息不能为空，请核查！");
            }
            PublishInfo tempPublishInfo = ((ICommon<PublishInfo>)IpublishInfo).GetInformation(tempTreadInfo.PublishID);
            if (tempPublishInfo == null)
            {
                throw new NoNullAllowedException("朋友圈发布信息不能为空，请核查！");
            }
            sqls.Add(DBService.DeleteSql(new TreadInfo()
            {
                ID = id
            }, out message1));
            sqls.Add(DBService.UpdateSql(new PublishInfo()
            {
                ID = tempTreadInfo.PublishID,
                TreadNum = tempPublishInfo.TreadNum - 1
            }, out message2));
            Miic.Friends.Notice.MessageInfo messageInfo = new Miic.Friends.Notice.MessageInfo()
            {
                ID = Guid.NewGuid().ToString(),
                PublisherID = tempTreadInfo.TreaderID,
                PublisherName = tempTreadInfo.TreaderName,
                PublishTime = tempTreadInfo.TreadTime,
                PublishID = tempTreadInfo.PublishID,
                NoticerID = tempPublishInfo.CreaterID,
                NoticerName = tempPublishInfo.CreaterName,
                ReadStatus = ((int)MiicReadStatusSetting.UnRead).ToString(),
                Source = ((int)BusinessTypeSetting.Moments).ToString(),
                MessageType = ((int)BehaviorTypeSetting.CancelTread).ToString()
            };
            if (tempTreadInfo.TreaderID != tempPublishInfo.CreaterID)
            {
                sqls.Add(DBService.InsertSql(messageInfo, out message3));
            }

            //积分
            sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
            {
                ID = Guid.NewGuid().ToString(),
                BusinessID = tempTreadInfo.ID,
                CreateTime = DateTime.Now,
                GetWay = ((int)GetWayTypeSetting.Behavior).ToString(),
                Score = -1 * ScoreConfig.Score.BehaviorScore,
                ServiceID = ScoreConfig.ServiceID,
                UserID = tempTreadInfo.TreaderID,
                UserName = tempTreadInfo.TreaderName
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
                Miic.Friends.Notice.MessageInfoDao.InsertCache(messageInfo);
                DeleteCache(o => o.ID == id);
                lock (syncRoot)
                {
                    if (PublishInfoDao.items.Find(o => o.ID == tempTreadInfo.PublishID) != null)
                    {
                        PublishInfoDao.items.Find(o => o.ID == tempTreadInfo.PublishID).TreadNum = tempPublishInfo.TreadNum - 1;
                    }
                }
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
        /// 查询当前登录人员对指定朋友圈信息的点踩信息
        /// </summary>
        /// <param name="momentsBehaviorView">用户行为（点踩）视图</param>
        /// <returns>点踩信息</returns>
        public TreadInfo GetUserTreadInfo(MyBehaviorView momentsBehaviorView)
        {
            Contract.Requires<ArgumentNullException>(momentsBehaviorView != null, "参数momentsBehaviorView:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(momentsBehaviorView.PublishID), "参数momentsBehaviorView.PublishID:不能为空");
            TreadInfo result = null;
            string message = string.Empty;
            MiicConditionCollections condition = momentsBehaviorView.visitor(this);
            MiicColumn threadAll = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<TreadInfo>());
            MiicColumnCollections columns = new MiicColumnCollections();
            columns.Add(threadAll);
            try
            {
                result = items.Find(o => o.PublishID == momentsBehaviorView.PublishID && o.TreaderID == momentsBehaviorView.UserID);
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
        /// 判断当前用户是否对制定朋友圈信息点过踩
        /// </summary>
        /// <param name="momentsBehaviorView"></param>
        /// <returns></returns>
        public bool IsTread(MyBehaviorView momentsBehaviorView)
        {
            Contract.Requires<ArgumentNullException>(momentsBehaviorView != null, "参数momentsBehaviorView:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(momentsBehaviorView.PublishID), "参数momentsBehaviorView.PublishID:不能为空");
            bool result = false;
            string message = string.Empty;
            MiicConditionCollections condition = momentsBehaviorView.visitor(this);
            MiicColumn threadID = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<TreadInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<TreadInfo, string>(o => o.ID));
            try
            {
                int count = dbService.GetCount<TreadInfo>(threadID, condition, out message);
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
