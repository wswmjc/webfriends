using Miic.Base;
using Miic.Base.Setting;
using Miic.Friends.Community;
using Miic.Friends.Community.Setting;
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
///行业圈子服务-成员子服务
/// </summary>
public partial class CommunityService
{
    
    [WebMethod(Description = "邀请成员加入行业圈子", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(CommunityInfo))]
    [GenerateScriptType(typeof(CommunityApplicationInfo))]
    public bool JoinMembers(List<CommunityApplicationInfo> memberApplications)
    {
       return IcommunityInfo.MemberInvite(memberApplications);
    }
    [WebMethod(MessageName="RemoveMember",Description = "删除行业圈子成员", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool RemoveMember(string memberID)
    {
        return ((ICommon<CommunityMember>)IcommunityInfo).Delete(memberID);
    }
    [WebMethod( Description = "退出我的行业圈子", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool Quit(string communityID) 
    {
        return IcommunityInfo.PersonQuit(communityID, this.UserID);
    }
    [WebMethod(Description = "批量移除行业圈子成员", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool RemoveMembers(string communityID,List<string> memberIDs)
    {
        return IcommunityInfo.RemoveMembers(communityID, memberIDs);
    }

    [WebMethod(Description = "行业圈子成员申请", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(CommunityApplicationInfo))]
    public bool MemberApplication(CommunityApplicationInfo communityApplicationInfo)
    {
        communityApplicationInfo.MemberID = this.UserID;
        communityApplicationInfo.MemberName = this.UserName;
        List<CommunityApplicationInfo> applications = new List<CommunityApplicationInfo>();
        applications.Add(communityApplicationInfo);
        return IcommunityInfo.MemberApply(applications);
    }

    [WebMethod(Description = "拒绝成员加入/邀请", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(ApproveView))]
    public bool Refuse(ApproveView approveView) 
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
                    bool isHandled = IcommunityInfo.IsApplicationHandled(approveView);
                    if (isHandled == false)
                    {
                        result = IcommunityInfo.MemberRefuse(approveView);
                    }
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

    [WebMethod(Description = "同意成员加入/邀请", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(ApproveView))]
    public bool Agree(ApproveView approveView) 
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
                    bool isHandled = IcommunityInfo.IsApplicationHandled(approveView);
                    if (isHandled == false)
                    {
                        //同意加入处理
                        bool isMember = IcommunityInfo.IsMember(approveView.MemberID, approveView.CommunityID);
                        if (isMember == false)
                        {
                            result = IcommunityInfo.MemberAgree(approveView);
                        }
                    }
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
    [WebMethod(Description = "审批忽略", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool Ignore(string ID) 
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
                    bool isHandled = IcommunityInfo.IsApplicationHandledByApplyID(ID);
                    if (isHandled == false)
                    {
                        result = IcommunityInfo.MemberIgnore(ID);
                    }
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
   
   
    [WebMethod(Description = "获取行业圈子成员列表", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(CommunityMember))]
    public string GetMemberInfoList(string communityID)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IcommunityInfo.GetDetailMemberInfoListByCommunityID(communityID);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.ID)],
                           MemberID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.MemberID)],
                           MemberName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.MemberName)],
                           MemberUrl = CommonService.GetManageFullUrl(dr[Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView, string>(o => o.UserUrl)].ToString()),
                           CommunityID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.CommunityID)],
                           IsAdmin = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.IsAdmin)],
                           JoinTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, DateTime?>(o => o.JoinTime)],
                           Remark = dr[Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView, string>(o => o.UserName)].ToString()
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
    
}