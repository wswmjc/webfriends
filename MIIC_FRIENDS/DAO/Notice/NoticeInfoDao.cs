using Miic.Base;
using Miic.Base.Setting;
using Miic.Common;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Common.Setting;
using Miic.Log;
using Miic.Manage.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Notice
{
    public partial class NoticeInfoDao:NoRelationCommon<NoticeInfo>,INoticeInfo<NoticeInfo>
    {
        private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
        private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
        bool ICommon<NoticeInfo>.Insert(NoticeInfo noticeInfo)
        {
            Contract.Requires<ArgumentNullException>(noticeInfo != null, "参数noticeInfo:不能为空");
            int count = 0;
            string message = string.Empty;
            bool result = false;
            try
            {
                result = dbService.Insert(noticeInfo, out count, out message);
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
                InsertCache(noticeInfo);
            }
            return result;
        }

        bool ICommon<NoticeInfo>.Update(NoticeInfo noticeInfo)
        {
            Contract.Requires<ArgumentNullException>(noticeInfo != null, "参数noticeInfo:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            try
            {
                result = dbService.Update(noticeInfo, out count, out message);
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
                DeleteCache(o => o.ID == noticeInfo.ID);
            }
            return result;
        }

        bool ICommon<NoticeInfo>.Delete(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            bool result = false;
            string message = string.Empty;
            int count = 0;
            try
            {
                result = dbService.Delete(new NoticeInfo()
                {
                    ID = id
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
                DeleteCache(o => o.ID == id);
            }
            return result;
        }

        NoticeInfo ICommon<NoticeInfo>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            NoticeInfo result = null;
            string message = string.Empty;
            try
            {
                result = items.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new NoticeInfo
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
                    result = Config.Serializer.Deserialize<NoticeInfo>(serializer);
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

        DataTable INoticeInfo<NoticeInfo>.GetMyNoticeInfoList(MyNoticeView myNoticeView, MiicPage page)
        {
            Contract.Requires<ArgumentNullException>(myNoticeView != null, "参数myNoticeView:不能为空");
            DataTable result = new DataTable();
            string message = string.Empty;
            MiicConditionCollections conditions=myNoticeView.visitor(this);
            List<MiicOrderBy> order = new List<MiicOrderBy>();
            order.Add(new MiicOrderBy()
            {
                Desc = true,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, DateTime?>(o => o.PublishTime)
            });
            conditions.order = order;
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<NoticeShowInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, string>(o => o.PublisherID),
                Config.Attribute.GetSqlTableNameByClassName<SimplePersonUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView, string>(o => o.UserID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.InnerJoin);
            try
            {
                if (page != null)
                {
                    result = dbService.GetInformationsPage(null, relation,conditions, page, out message);
                }
                else 
                {
                    result = dbService.GetInformations(null, relation,conditions, out message);
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

        int INoticeInfo<NoticeInfo>.GetMyNoticeInfoCount(MyNoticeView myNoticeView)
        {
            Contract.Requires<ArgumentNullException>(myNoticeView != null, "参数myNoticeView:不能为空");
            int result = 0;
            string message = string.Empty;
            MiicConditionCollections conditions = myNoticeView.visitor(this);
            try
            {
                result = dbService.GetCount<NoticeInfo>(null, conditions, out message);
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

        bool INoticeInfo<NoticeInfo>.ReadAllNotice(string noticerID, BusinessTypeSetting businessType)
        {
            bool result = false;
            int count = 0;
            string message = string.Empty;
            MiicConditionCollections conditions = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition sourceCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<NoticeInfo, string>(o => o.Source),
                ((int)businessType).ToString(),
                 DbType.String,
                  MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(MiicDBLogicSetting.No,sourceCondition));
            MiicCondition noticerIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<NoticeInfo, string>(o => o.NoticerID),
                noticerID,
                 DbType.String,
                  MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(noticerIDCondition));
            try
            {
                result = dbService.UpdateConditions<NoticeInfo>(new NoticeInfo()
                {
                    ReadStatus = ((int)MiicReadStatusSetting.Read).ToString()
                }, conditions, out count, out message);
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
                items.RemoveAll(o => o.NoticerID == noticerID && o.Source == ((int)businessType).ToString());
            }
            return result;
        }
        /// <summary>
        /// 获取我的/某人@离线未读信息
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns>离线信息列表</returns>
        public DataTable GetPersonOfflineNoticeList(string userID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(userID), "参数userID:不能为空");
            DataTable result = new DataTable();
            string message = string.Empty;
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition userIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, string>(o => o.NoticerID),
                userID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, userIDCondition));
            MiicCondition readStatusCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, string>(o => o.ReadStatus),
                ((int)MiicReadStatusSetting.UnRead).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(readStatusCondition));
            List<MiicOrderBy> orders = new List<MiicOrderBy>();
            orders.Add(new MiicOrderBy()
            {
                Desc = true,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, DateTime?>(o => o.PublishTime)
            });
            condition.order = orders;
            MiicColumnCollections columns = new MiicColumnCollections();
            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<NoticeShowInfo>());
            columns.Add(column);
            try
            {
                result = dbService.GetInformations<NoticeShowInfo>(columns, condition, out message);
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
        /// 反查未读的NoticeID
        /// </summary>
        /// <param name="unreadNoticeView">未读通知视图</param>
        /// <returns></returns>
        public DataTable GetUnReadNoticeID(UnreadNoticeView unreadNoticeView)
        {
            Contract.Requires<ArgumentNullException>(unreadNoticeView != null, "参数unreadNoticeView:不能为空");
            DataTable result = new DataTable();
            string message = string.Empty;
            MiicConditionCollections condition = unreadNoticeView.visitor(this);
            MiicColumnCollections columns = new MiicColumnCollections();
            MiicColumn idColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<NoticeInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<NoticeInfo, string>(o => o.ID));
            columns.Add(idColumn);
            MiicColumn noticerIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<NoticeInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<NoticeInfo, string>(o => o.NoticerID));
            columns.Add(noticerIDColumn);
            try
            {
                result = dbService.GetInformations<NoticeInfo>(columns, condition, out message);

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
