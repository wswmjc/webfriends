using Miic.Base;
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

namespace Miic.Friends.Community
{
    public partial class MessageInfoDao : RelationCommon<TopicInfo, MessageInfo>, IMessageInfo
    {
        private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
        private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;

        private object syncRoot = new object();

        public MessageInfoDao()
        {

        }

        bool ICommon<MessageInfo>.Insert(MessageInfo messageInfo)
        {
            Contract.Requires<ArgumentNullException>(messageInfo != null, "参数messageInfo：不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(messageInfo.ID), "参数messageInfo.ID：不能为空！");
            bool result = false;
            List<string> sqls = new List<string>();
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            string message3 = string.Empty;
            sqls.Add(DBService.InsertSql(messageInfo, out message1));
            try
            {
                TopicInfo temp = ((ICommon<TopicInfo>)this).GetInformation(messageInfo.TopicID);
                if (temp == null)
                {
                    throw new NoNullAllowedException("temp发布信息不能为空,请核查！");
                }
                sqls.Add(DBService.UpdateSql(new TopicInfo()
                {
                    ID = messageInfo.TopicID,
                    MessageCount = temp.MessageCount + 1
                }, out message2));
                if (!string.IsNullOrEmpty(messageInfo.ToCommenterID)) 
                {
                    NoticeInfo noticeInfo = new NoticeInfo()
                    {
                        ID = Guid.NewGuid().ToString(),
                        NoticerID = messageInfo.ToCommenterID,
                        NoticerName = messageInfo.ToCommenterName,
                        Source = ((int)Miic.Friends.Common.Setting.BusinessTypeSetting.Community).ToString(),
                        NoticeType = ((int)Miic.Friends.Notice.Setting.NoticeTypeSetting.Message).ToString(),
                        PublisherID = messageInfo.FromCommenterID,
                        PublisherName = messageInfo.FromCommenterName,
                        PublishID = messageInfo.ID,
                        PublishTime = messageInfo.CommentTime,
                        ReadStatus = ((int)Miic.Base.Setting.MiicReadStatusSetting.UnRead).ToString()
                    };

                    if (noticeInfo.NoticerID != noticeInfo.PublisherID)
                    {
                        sqls.Add(DBService.InsertSql(noticeInfo, out message3));
                    }

                }
               
                result = dbService.excuteSqls(sqls, out message);
                if (result == true)
                {
                    InsertCache(messageInfo);
                    lock (syncRoot)
                    {
                        MessageInfoDao.items.Find(o => o.ID == messageInfo.TopicID).MessageCount = temp.MessageCount + 1;
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
            if (result == true)
            {
                InsertCache(messageInfo);
            }
            return result;
        }

        bool ICommon<MessageInfo>.Update(MessageInfo messageInfo)
        {
            Contract.Requires<ArgumentNullException>(messageInfo != null, "参数messageInfo:不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(messageInfo.ID), "参数messageInfo.ID:不能为空，因为是主键");
            int count = 0;
            string message = string.Empty;
            bool result = false;
            try
            {
                result = dbService.Update(messageInfo, out count, out message);
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
                DeleteCache(o => o.ID == messageInfo.ID);
            }
            return result;
        }

        bool ICommon<MessageInfo>.Delete(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            bool result = false;
            string message = string.Empty;
            MessageInfo messageInfo = ((ICommon<MessageInfo>)this).GetInformation(id);
            TopicInfo topicInfo = ((ICommon<TopicInfo>)this).GetInformation(messageInfo.TopicID);
            if (messageInfo != null && topicInfo != null)
            {
                try
                {
                    List<string> sqls = new List<string>();
                    sqls.Add(DBService.UpdateSql<TopicInfo>(new TopicInfo()
                    {
                        ID = messageInfo.TopicID,
                        MessageCount = topicInfo.MessageCount - 1
                    }, out message));
                    sqls.Add(DBService.DeleteSql<MessageInfo>(new MessageInfo()
                    {
                        ID = id
                    }, out message));
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
                    lock (syncRoot)
                    {
                        items.Find(o => o.ID == topicInfo.ID).MessageCount = topicInfo.MessageCount - 1;
                    }
                }
            }
            return result;
        }

        MessageInfo ICommon<MessageInfo>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            MessageInfo result = null;
            string message = string.Empty;
            try
            {
                result = subitems.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new MessageInfo
                    {
                        ID = id
                    }, out message);
                    if (result != null)
                    {
                        List<MessageInfo> messageList = new List<MessageInfo>();
                        messageList.Add(result);
                        InsertCaches(messageList);
                    }
                }
                else
                {
                    string serializer = Config.Serializer.Serialize(result);
                    result = Config.Serializer.Deserialize<MessageInfo>(serializer);
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
        /// 根据讨论ID获取所有message信息
        /// </summary>
        /// <param name="topicID">讨论ID</param>
        /// <returns>所有message信息</returns>
        public DataTable GetMessageListByTopicID(string topicID, MiicPage page)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(topicID), "参数topicID:不能为空");
            DataTable result = new DataTable();
            string message = string.Empty;
            string sql = string.Empty;
            Cookie cookie = new Cookie();
            string UserID = cookie.GetCookie("SNS_ID", out message);
            if (string.IsNullOrEmpty(UserID))
            {
                throw new Miic.MiicException.MiicCookieArgumentNullException("UserID不能为空，Cookie失效");
            }
            sql += "SELECT * FROM GetCommunityMessageListWithAddress('" + UserID + "','" + topicID + "') ";
            if (page != null)
            {
                sql = "WITH INFO_PAGE AS ( SELECT row_number() OVER ( ORDER BY  Temp.COMMENT_TIME DESC) as row,Temp.*  FROM ( " + sql;
                sql += " ) as Temp) ";
                sql += "SELECT * FROM INFO_PAGE where row between " + page.pageStart + " and " + page.pageEnd + ";";
            }
            else
            {
                sql += " ORDER BY COMMENT_TIME DESC";
            }
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

        /// <summary>
        /// 根据讨论ID获取messagecount
        /// </summary>
        /// <param name="topicID">讨论ID</param>
        /// <returns>messagecount</returns>
        public int GetMessageCountByTopicID(string topicID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(topicID), "参数topicID:不能为空");
            string message = string.Empty;
            int result = 0;
            MiicCondition topicCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<MessageInfo, string>(o => o.TopicID),
               topicID,
               DbType.String,
               DB.Setting.MiicDBOperatorSetting.Equal);
            MiicColumn idColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<MessageInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<MessageInfo, string>(o => o.ID));
            try
            {
                result = dbService.GetCount<MessageInfo>(idColumn, new MiicConditionSingle(topicCondition), out message);
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
