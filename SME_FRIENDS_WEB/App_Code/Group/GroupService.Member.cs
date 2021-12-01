using Miic.Base;
using Miic.DB.SqlObject;
using Miic.Friends.AddressBook;
using Miic.Friends.Group;
using Miic.Friends.SimpleGroup;
using Miic.Log;
using Miic.Manage.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Transactions;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;

/// <summary>
///讨论组服务-成员子服务
/// </summary>
public partial class GroupService
{
    [WebMethod(Description = "移除讨论组成员", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool RemoveMember(string groupID,List<string> members)
    {
       
        return IgroupInfo.Delete(groupID,members);
    }
    [WebMethod(Description = "添加讨论组成员", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(GroupMember))]
    public bool AddMember(List<GroupMember> members) 
    {
        bool result = false;
        try
        {
            TransactionOptions transactionOptions = new TransactionOptions();
            transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            transactionOptions.Timeout = new TimeSpan(0, 1, 0);
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, transactionOptions, EnterpriseServicesInteropOption.Automatic))
            {
                try
                {
                    List<GroupMember> tempMembers = IgroupInfo.GetGroupMemberList(members[0].GroupID);
                    foreach (var item in members)
                    {
                        if (tempMembers.Find(m => m.MemberID == item.MemberID) != null)
                        {
                            members.Remove(item);
                        }
                    }
                    result = IgroupInfo.Insert(members[0].GroupID, members);
                    ts.Complete();
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
                    ts.Dispose();
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

    [WebMethod(Description = "设置备注", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(Miic.Friends.Group.SetRemarkView))]
    public bool SetRemark(Miic.Friends.Group.SetRemarkView remarkView)
    {
        return IgroupInfo.SetRemark(remarkView);
    }

    [WebMethod(Description = "获取小组成员列表", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetMemberInfoList(string groupID)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IgroupInfo.GetDetailMemberInfoListByGroupID(groupID);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.ID)],
                           MemberID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.MemberID)],
                           MemberName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.MemberName)],
                           MemberUrl = CommonService.GetManageFullUrl(dr[Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView, string>(o => o.UserUrl)].ToString()),
                           GroupID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.GroupID)],
                           JoinTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, DateTime?>(o => o.JoinTime)],
                           Remark = dr[Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView, string>(o => o.UserName)].ToString()
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }

    [WebMethod(Description = "获取待邀请成员列表", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MySimpleGroupSearchView))]
    [GenerateScriptType(typeof(MiicPage))]
    public string GetInvitingAddressList(MySimpleGroupSearchView groupSearchView, MiicPage page)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IgroupInfo.GetInvitingAddressList(groupSearchView, page);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           UserID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserID)],
                           UserCode = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserName)],
                           UserName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView, string>(o => o.UserName)],
                           UserUrl = CommonService.GetManageFullUrl(dr[Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView, string>(o => o.UserUrl)].ToString()),
                           Remark = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.Remark)]
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }

    [WebMethod(Description = "获取待邀请成员数量", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MySimpleGroupSearchView))]
    public int GetInvitingAddressCount(MySimpleGroupSearchView groupSearchView)
    {
        return IgroupInfo.GetInvitingAddresserCount(groupSearchView);
    }
}