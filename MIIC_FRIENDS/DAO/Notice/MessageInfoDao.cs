using Miic.Base;
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
    public partial class MessageInfoDao : NoRelationCommon<MessageInfo>, INoticeInfo<MessageInfo>
    {
        private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
        private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
        bool ICommon<MessageInfo>.Insert(MessageInfo messageInfo)
        {
            Contract.Requires<ArgumentNullException>(messageInfo != null, "参数messageInfo:不能为空");
            int count = 0;
            string message = string.Empty;
            bool result = false;
            try
            {
                result = dbService.Insert(messageInfo, out count, out message);
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
            Contract.Requires<ArgumentNullException>(messageInfo != null, "参数messageInfo:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
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
            int count = 0;
            try
            {
                result = dbService.Delete(new MessageInfo()
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

        MessageInfo ICommon<MessageInfo>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            MessageInfo result = null;
            string message = string.Empty;
            try
            {
                result = items.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new MessageInfo
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



        DataTable INoticeInfo<MessageInfo>.GetMyNoticeInfoList(MyNoticeView myNoticeView, MiicPage page)
        {
            Contract.Requires<ArgumentNullException>(myNoticeView != null, "参数myNoticeView:不能为空");
            DataTable result = new DataTable();
            string message = string.Empty;
            MiicConditionCollections conditions = myNoticeView.visitor(this);
            List<MiicOrderBy> order = new List<MiicOrderBy>();
            order.Add(new MiicOrderBy()
            {
                Desc = true,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<MessageInfo, DateTime?>(o => o.PublishTime)
            });
            conditions.order = order;
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<MessageInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<MessageInfo, string>(o => o.PublisherID),
                Config.Attribute.GetSqlTableNameByClassName<SimplePersonUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView, string>(o => o.UserID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.InnerJoin);

            try
            {
                if (page != null)
                {
                    result = dbService.GetInformationsPage(null, relation, conditions, page, out message);
                }
                else
                {
                    result = dbService.GetInformations(null, relation, conditions, out message);
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

        int INoticeInfo<MessageInfo>.GetMyNoticeInfoCount(MyNoticeView myNoticeView)
        {
            Contract.Requires<ArgumentNullException>(myNoticeView != null, "参数myNoticeView:不能为空");
            int result = 0;
            string message = string.Empty;
            MiicConditionCollections conditions = myNoticeView.visitor(this);
            try
            {
                result = dbService.GetCount<MessageInfo>(null, conditions, out message);
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

        public bool ReadAllNotice(string noticerID, BusinessTypeSetting type)
        {
            throw new NotImplementedException();
        }
    }
}
