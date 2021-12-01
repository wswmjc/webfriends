using Miic.Base;
using Miic.Base.Setting;
using Miic.DB;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Notice;
using Miic.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Group
{
    public partial class MessageInfoDao : RelationCommon<TopicInfo, MessageInfo>, IMessageInfo
    {
        private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
        private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
        public static bool DeleteCacheByGroupID(string groupID)
        {
            bool result = false;
            try
            {
                List<TopicInfo> cacheTopicList = items.FindAll(o => o.GroupID == groupID);
                items.RemoveAll(o => o.GroupID == groupID);
                foreach (var item in cacheTopicList.AsEnumerable())
                {
                    subitems.RemoveAll(o => o.TopicID == item.ID);
                }
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
            return result;
        }
        bool ICommon<TopicInfo>.Insert(TopicInfo topicInfo)
        {
            Contract.Requires<ArgumentNullException>(topicInfo != null, "参数topicInfo:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            try
            {
                //有效性
                topicInfo.Valid = ((int)MiicValidTypeSetting.Valid).ToString();
                //评论总数
                topicInfo.MessageCount = 0;
                result = dbService.Insert(topicInfo, out count, out message);
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
                InsertCache(topicInfo);
            }
            return result;
        }

        bool ICommon<TopicInfo>.Update(TopicInfo topicInfo)
        {
            Contract.Requires<ArgumentNullException>(topicInfo != null, "参数topicInfo:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            try
            {
                result = dbService.Update(topicInfo, out count, out message);
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
                DeleteCache(o => o.ID == topicInfo.ID);
            }
            return result;
        }
        /// <summary>
        /// 删除Topic信息（用户客户删除）
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>Yes/No</returns>
        /// <remarks>如果MessageCount=0,则直接删除，否则设置失效</remarks>
        bool ICommon<TopicInfo>.Delete(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            bool result = false;
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            string message3 = string.Empty;
            List<string> sqls = new List<string>();
            TopicInfo temp = ((ICommon<TopicInfo>)this).GetInformation(id);
            try
            {
                if (temp != null)
                {
                    if (temp.MessageCount > 0)
                    {
                        sqls.Add(DBService.UpdateSql<TopicInfo>(new TopicInfo()
                        {
                            ID = id,
                            Valid = ((int)MiicValidTypeSetting.InValid).ToString(),
                            EndTime = DateTime.Now
                        }, out message1));
                    }
                    else
                    {
                        sqls.Add(DBService.DeleteSql<TopicInfo>(new TopicInfo()
                        {
                            ID = id
                        }, out message2));
                    }

                    //提醒级联
                    MiicCondition noticeIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<NoticeInfo, string>(o => o.PublishID),
                       id,
                       DbType.String,
                       MiicDBOperatorSetting.Equal);
                    MiicConditionSingle noticeCondition = new MiicConditionSingle(noticeIDCondition);
                    sqls.Add(DBService.DeleteConditionSql<NoticeInfo>(noticeCondition, out message3));

                    result = dbService.excuteSqls(sqls, out message);
                }
                else
                {
                    throw new ArgumentException("参数id：赋值无效");
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
                DeleteCache(true, o => o.ID == id, o => o.TopicID == id);
            }
            return result;
        }
        /// <summary>
        /// 是否有组讨论信息
        /// </summary>
        /// <param name="groupID">讨论组ID</param>
        /// <returns>Yes/No</returns>
        public bool HasGroupTopic(string groupID) 
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(groupID), "参数groupID:不能为空");
            bool result = false;
            if (items.Exists(o => o.GroupID == groupID) == true)
            {
                result = true;
            }
            else 
            {
                string message = string.Empty;
                MiicCondition groupIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<TopicInfo, string>(o => o.GroupID),
                    groupID,
                    DbType.String,
                    MiicDBOperatorSetting.Equal);
                MiicConditionSingle condition = new MiicConditionSingle(groupIDCondition);
                MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<TopicInfo>(),
                    Config.Attribute.GetSqlColumnNameByPropertyName<TopicInfo, string>(o => o.ID));
               int count= dbService.GetCount<TopicInfo>(column, condition, out message, true);
               if (count > 0) 
               {
                   result = true;
               }
            }
               
            return result;
        }
        /// <summary>
        /// 删除Topic信息(管理员使用)
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>Yes/No</returns>
        /// <remarks>包括Message的内容，级联</remarks>
        bool IMessageInfo.DeepDelete(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            bool result = false;
            string message = string.Empty,
                message1 = string.Empty,
                message2 = string.Empty;
            List<string> sqls = new List<string>();
            MiicCondition groupIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<TopicInfo, string>(o => o.GroupID),
                id,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            MiicConditionSingle condition = new MiicConditionSingle(groupIDCondition);
            sqls.Add(DBService.DeleteConditionSql<MessageInfo>(condition, out message1));
            sqls.Add(DBService.DeleteSql(new TopicInfo() { ID = id }, out message2));
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
                DeleteCache(true, o => o.ID == id, o => o.TopicID == id);
            }
            return result;
        }
        TopicInfo ICommon<TopicInfo>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            TopicInfo result = null;
            string message = string.Empty;
            try
            {
                result = items.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new TopicInfo
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
                    result = Config.Serializer.Deserialize<TopicInfo>(serializer);
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




        DataTable IMessageInfo.Search(TopicSearchView searchView, MiicPage page)
        {
            Contract.Requires<ArgumentNullException>(searchView != null, "参数searchView:不能为空");
            DataTable result = new DataTable();
            string message = string.Empty;
            try
            {
                if (page == null)
                {
                    Dictionary<String, String> paras = new Dictionary<String, String>();
                    paras.Add("USER_ID", searchView.UserID);
                    paras.Add("GROUP_ID", searchView.GroupID);
                    paras.Add("KEYWORD", searchView.Keyword);
                    paras.Add("PAGE_START", string.Empty);
                    paras.Add("PAGE_END", string.Empty);
                    string storeProcedureName = "SearchGroupTopic";
                    result = dbService.QueryStoredProcedure<string>(storeProcedureName, paras, out message);
                }
                else
                {
                    Dictionary<String, String> paras = new Dictionary<String, String>();
                    paras.Add("USER_ID", searchView.UserID);
                    paras.Add("GROUP_ID", searchView.GroupID);
                    paras.Add("KEYWORD", searchView.Keyword);
                    paras.Add("PAGE_START", page.pageStart);
                    paras.Add("PAGE_END", page.pageEnd);
                    string storeProcedureName = "SearchGroupTopic";
                    result = dbService.QueryStoredProcedure<string>(storeProcedureName, paras, out message);
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

        int IMessageInfo.GetSearchCount(TopicSearchView searchView)
        {
            Contract.Requires<ArgumentNullException>(searchView != null, "参数searchView:不能为空");
            int result = 0;
            string message = string.Empty;
            MiicConditionCollections conditions = searchView.visitor(this);
            try
            {
                result = dbService.GetCount<TopicInfo>(null, conditions, out message);
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
        /// 讨论组讨论新增
        /// </summary>
        /// <param name="topicInfo">讨论对象</param>
        /// <param name="noticeUserView">提醒人对象</param>
        /// <returns></returns>
        public bool Insert(TopicInfo topicInfo, NoticeUserView noticeUserView)
        {
            Contract.Requires<ArgumentNullException>(topicInfo != null, "参数topicInfo:不能为空");
            bool result = false;
            List<string> sqls = new List<string>();
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            /*------------------------------微博初始化值-----------------------------------*/
            //有效性
            topicInfo.Valid = ((int)MiicValidTypeSetting.Valid).ToString();
            //评论总数
            topicInfo.MessageCount = 0;
            /*------------------------------用户初始化值-----------------------------------*/

            sqls.Add(DBService.InsertSql(topicInfo, out message1));

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
                        PublisherID = topicInfo.CreaterID,
                        PublisherName = topicInfo.CreaterName,
                        PublishID = topicInfo.ID,
                        PublishTime = topicInfo.CreateTime,
                        ReadStatus = ((int)MiicReadStatusSetting.UnRead).ToString()
                    }, out message2));
                }
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
                InsertCache(topicInfo);
            }
            return result;
        }

        /// <summary>
        /// 讨论组讨论更新
        /// </summary>
        /// <param name="topicInfo">讨论对象</param>
        /// <param name="noticeUserView">提醒人对象</param>
        /// <returns></returns>
        public bool Update(TopicInfo topicInfo, NoticeUserView noticeUserView)
        {
            Contract.Requires<ArgumentNullException>(topicInfo != null, "参数topicInfo:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(topicInfo.ID), "参数topicInfo.ID:不能为空");
            bool result = false;
            List<string> sqls = new List<string>();
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            try
            {
                TopicInfo tempTopicInfo = ((ICommon<TopicInfo>)this).GetInformation(topicInfo.ID);
                sqls.Add(DBService.UpdateSql<TopicInfo>(topicInfo, out message1));

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
                            PublisherID = topicInfo.CreaterID,
                            PublisherName = topicInfo.CreaterName,
                            PublishID = topicInfo.ID,
                            PublishTime = topicInfo.CreateTime,
                            ReadStatus = ((int)MiicReadStatusSetting.UnRead).ToString()
                        }, out message2));
                    }

                }

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
                DeleteCache(true, o => o.ID == topicInfo.ID, o => o.TopicID == topicInfo.ID);
            }
            return result;
        }
    }
}
