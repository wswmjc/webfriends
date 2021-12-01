using Miic.Base;
using Miic.Base.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Notice;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using Miic.Manage.User;
using Miic.Friends.Common.Setting;

/// <summary>
///@服务
/// </summary>
[WebService(Namespace = "http://pyq.mictalk.cn/")]
[WebServiceBinding(ConformsTo = WsiProfiles.None)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[ScriptService]
public class NoticeService : WebService
{
    private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
    private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
    private static readonly INoticeInfo<NoticeInfo> InoticeInfo = new NoticeInfoDao(); 
    public string UserID { get; private set; }
    public string UserName { get; private set; }
    public NoticeService()
    {
        string message = string.Empty;
        Cookie cookie = new Cookie();
        this.UserID = cookie.GetCookie("SNS_ID", out message);
        this.UserName = HttpUtility.UrlDecode(cookie.GetCookie("SNS_UserName", out message));
    }
    [WebMethod(Description = "获取我的@离线信息", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetMyOfflineNoticeList()
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = ((NoticeInfoDao)InoticeInfo).GetPersonOfflineNoticeList(this.UserID);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, string>(o => o.ID)],
                           PublishID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, string>(o => o.ShowID)],
                           Content = dr[Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, string>(o => o.Content)],
                           UserID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, string>(o => o.PublisherID)],
                           UserName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, string>(o => o.PublisherName)],
                           BusinessType = dr[Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, string>(o => o.Source)],
                           NoticeType = dr[Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, string>(o => o.NoticeType)]
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }

    [WebMethod(Description = "查询@信息", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MyNoticeView))]
    [GenerateScriptType(typeof(MiicPage))]
    public string Search(MyNoticeView myNoticeView, MiicPage page)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = InoticeInfo.GetMyNoticeInfoList(myNoticeView, page);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
              {
                  ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, string>(o => o.ID)],
                  PublishID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, string>(o => o.ShowID)],
                  Source = dr[Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, string>(o => o.Source)],
                  Content = dr[Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, string>(o => o.Content)],
                  PublishType=dr[Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo,string>(o=>o.PublishType)],
                  UserID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, string>(o => o.PublisherID)],
                  UserName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, string>(o => o.PublisherName)],
                  UserUrl = CommonService.GetManageFullUrl(dr[Config.Attribute.GetSqlColumnNameByPropertyName<MiicSocialUserInfo, string>(o => o.MicroUserUrl)].ToString()),
                  PublishTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<NoticeShowInfo, DateTime?>(o => o.PublishTime)]
              };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
    [WebMethod(Description = "查询@信息数", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MyNoticeView))]
    public int GetSearchCount(MyNoticeView myNoticeView)
    {
        return InoticeInfo.GetMyNoticeInfoCount(myNoticeView);
    }
    [WebMethod(Description = "阅读@信息", BufferResponse = true)]
    public bool ReadNotice(string id)
    {
        return InoticeInfo.Update(new NoticeInfo()
        {
            ID = id,
            ReadTime = DateTime.Now,
            ReadStatus = ((int)MiicReadStatusSetting.Read).ToString()
        });
    }
    [WebMethod(Description = "忽略@信息", BufferResponse = true)]
    public bool IgnoreNotice(string id)
    {
        return InoticeInfo.Update(new NoticeInfo()
        {
            ID = id,
            ReadTime = DateTime.Now,
            ReadStatus = ((int)MiicReadStatusSetting.Ignore).ToString()
        });
    }

    [WebMethod(Description = "阅读所有@信息", BufferResponse = true)]
    public bool ReadAllNotice(BusinessTypeSetting type)
    {
        return InoticeInfo.ReadAllNotice(this.UserID, type);
    }
}
