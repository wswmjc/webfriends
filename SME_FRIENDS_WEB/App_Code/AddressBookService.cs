using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using Miic.Friends.AddressBook;
using Miic.DB.SqlObject;
using Miic.Base;
using Miic.Friends.SimpleGroup;
using System.Data;
using Miic.Base.Setting;
using Miic.Manage.User;
using Miic.Friends.AddressBook.Setting;
/// <summary>
/// 通讯录服务
/// </summary>
[WebService(Namespace = "http://pyq.mictalk.cn/")]
[WebServiceBinding(ConformsTo = WsiProfiles.None)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[ScriptService]
public class AddressBookService : WebService
{
    private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
    private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
    private readonly IAddressBookInfo IaddressBookInfo = new AddressBookInfoDao();
    public string UserID { get; private set; }
    public string UserName { get; private set; }
    public AddressBookService()
    {
        string message = string.Empty;
        Cookie cookie = new Cookie();
        this.UserID = cookie.GetCookie("SNS_ID", out message);
        this.UserName = HttpUtility.UrlDecode(cookie.GetCookie("SNS_UserName", out message));
    }

    [WebMethod(Description = "同意添加通讯录申请", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(ApproveView))]
    public bool Agree(ApproveView approveView)
    {
        return IaddressBookInfo.Agree(approveView);
    }
    [WebMethod(Description = "添加通讯录申请", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(AddressBookApplicationInfo))]
    public new bool Application(AddressBookApplicationInfo applicationInfo)
    {
        applicationInfo.MyUserID = this.UserID;
        applicationInfo.ApplicationTime = DateTime.Now;
        applicationInfo.ResponseStatus = ((int)ApplyStatusSetting.Apply).ToString();
        return ((ICommon<AddressBookApplicationInfo>)IaddressBookInfo).Insert(applicationInfo);
    }
    [WebMethod(Description = "拒绝通讯录申请", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(ApproveView))]
    public bool Refuse(ApproveView approveView)
    {
        return IaddressBookInfo.Refuse(approveView);
    }
    [WebMethod(Description = "忽略通讯录申请", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool Ignore(string ID) 
    {
        return ((ICommon<AddressBookApplicationInfo>)IaddressBookInfo).Update(new AddressBookApplicationInfo()
        {
            ID = ID,
            ResponseStatus = ((int)ApplyStatusSetting.Ignore).ToString(),
            ResponseTime = DateTime.Now
        });
    }
    [WebMethod(MessageName = "SimplexRemove", Description = "单向移除通讯录", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool SimplexRemove(string ID)
    {
        return ((ICommon<AddressBookInfo>)IaddressBookInfo).Delete(ID);
    }
    [WebMethod(MessageName = "DuplexRemove", Description = "双向移除通讯录", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool DuplexRemove(string userID) 
    {
        string firstUserID = this.UserID;
        string secondUserID = userID;
        return IaddressBookInfo.Remove(firstUserID, secondUserID);
    }
    [WebMethod(Description = "搜索我的通讯录", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MySimpleGroupSearchView))]
    [GenerateScriptType(typeof(MiicPage))]
    public string Search(MySimpleGroupSearchView searchView, MiicPage page)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IaddressBookInfo.Search(searchView, page);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.ID)],
                           AddresserID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserID)],
                           AddresserName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserName)],
                           AddresserNickName = dr["NickName"],
                           AddresserUrl = CommonService.GetManageFullUrl(dr[Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView, string>(o => o.UserUrl)].ToString()),
                           IsBlackList = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.IsBlackList)],
                           CanSeeMe = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.CanSeeMe)],
                           CanSeeMeTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, DateTime?>(o => o.CanSeeMeTime)],
                           CanSeeAddresser = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.CanSeeAddresser)],
                           CanSeeAddresserTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, DateTime?>(o => o.CanSeeAddresserTime)],
                           OftenUsed = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.OftenUsed)],
                           Remark = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.Remark)]
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }

    [WebMethod(Description = "搜索我的通讯录数", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MySimpleGroupSearchView))]
    public int GetSearchCount(MySimpleGroupSearchView searchView)
    {
        return IaddressBookInfo.GetSearchCount(searchView);
    }
    [WebMethod(Description = "搜索我的通讯录的黑名单", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MySimpleGroupSearchView))]
    [GenerateScriptType(typeof(MiicPage))]
    public string BlackListSearch(MySimpleGroupSearchView searchView, MiicPage page)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IaddressBookInfo.GetPersonBlackListInfos(searchView, page);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.ID)],
                           AddresserID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserID)],
                           AddresserName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserName)],
                           AddresserUrl = CommonService.GetManageFullUrl(dr[Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView, string>(o => o.UserUrl)].ToString()),
                           Remark = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.Remark)]
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
    [WebMethod(Description = "搜索我的通讯录黑名单数", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MySimpleGroupSearchView))]
    public int GetBlackListSearchCount(MySimpleGroupSearchView searchView)
    {
        return IaddressBookInfo.GetPersonBlackListCount(searchView);
    }
    [WebMethod(Description = "添加备注", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(SetRemarkView))]
    public bool SetRemark(SetRemarkView remarkView)
    {
        return IaddressBookInfo.SetRemark(remarkView);
    }
    [WebMethod(Description = "设置是否经常联系", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(SetOftenUsedView))]
    public bool SetOftenUsed(SetOftenUsedView oftenUsedView) 
    {
        return IaddressBookInfo.SetOftenUsed(oftenUsedView);
    }
    [WebMethod(Description = "能看我", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(SetCanSeeMeView))]
    public bool SetCanSeeMe(SetCanSeeMeView canSeeMeView)
    {
        return IaddressBookInfo.SetCanSeeMe(canSeeMeView);
    }
    [WebMethod(Description = "看朋友", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(SetCanSeeAddresserView))]
    public bool SetCanSeeAddresser(SetCanSeeAddresserView canSeeAddresserView)
    {
        return IaddressBookInfo.SetCanSeeAddresser(canSeeAddresserView);
    }
    [WebMethod(Description = "获取经常使用的通讯录", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string ShowMyOffenUsedAddressBookList(int top)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IaddressBookInfo.GetOffenUsedAddressBookList(this.UserID, top);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           AddresserID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserID)],
                           AddresserName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserName)],
                           AddresserUrl = CommonService.GetManageFullUrl(dr[Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView, string>(o => o.UserUrl)].ToString()),
                           OftenUsed = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.OftenUsed)]
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
    [WebMethod(Description = "加入黑名单", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool AddBlackList(string ID)
    {
        return IaddressBookInfo.Update(new AddressBookInfo()
        {
            ID = ID,
            OftenUsed=((int)MiicYesNoSetting.No).ToString(),
            IsBlackList = ((int)MiicYesNoSetting.Yes).ToString(),
        });
    }
    [WebMethod(Description = "移除黑名单", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool RemoveBlackList(string ID)
    {
        return IaddressBookInfo.Update(new AddressBookInfo()
        {
            ID = ID,
            IsBlackList = ((int)MiicYesNoSetting.No).ToString(),
        });
    }
    [WebMethod(Description = "获取我的通知列表", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetMyValidationMessageList()
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IaddressBookInfo.GetPersonValidationMessageInfos(this.UserID);
        if (dt.Rows.Count > 0) 
        {
            var temp = from dr in dt.AsEnumerable()
                       select new 
                       {
                           ID=dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo,string>(o=>o.ID)],
                           UserID=dr[Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView,string>(o=>o.UserID)],
                           UserUrl=CommonService.GetManageFullUrl(dr[Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView,string>(o=>o.UserUrl)].ToString()),
                           UserName=dr[Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView,string>(o=>o.UserName)],
                           UserType=dr[Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView,string>(o=>o.UserType)],
                           ApplicationTime=dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo,DateTime?>(o=>o.ApplicationTime)],
                           ResponseTime = Convert.IsDBNull(dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo, DateTime?>(o => o.ResponseTime)]) == true ? null : (DateTime?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo, DateTime?>(o => o.ResponseTime)],
                           ResponseStatus=dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo,string>(o=>o.ResponseStatus)],
                           Remark=dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo,string>(o=>o.Remark)].ToString()
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
    [WebMethod(Description = "获取我的通知列表数", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public int GetMyValidationMessageCount() 
    {
        return IaddressBookInfo.GetPersonValidationMessageCount(this.UserID);
    }

    [WebMethod(Description = "是否联系人对自己可见", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool CanSeeAddresser(string addresserID)
    {
        return IaddressBookInfo.CanSeeAddresser(this.UserID, addresserID);
    }
}
