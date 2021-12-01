using Miic.Base;
using Miic.Common;
using Miic.DB.SqlObject;
using Miic.Friends.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using Miic.Friends.Moments;
using Miic.Friends.Notice;
using Miic.Friends.Common.Setting;
using Miic.Base.Setting;
using System.IO;
using Miic.Log;
using System.Data;
using System.Transactions;
using Miic.Base.ConfigSection;
using System.Web.Configuration;

/// <summary>
/// 朋友圈服务
/// </summary>
[WebService(Namespace = "http://pyq.mictalk.cn/")]
[WebServiceBinding(ConformsTo = WsiProfiles.None)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[ScriptService]
public partial class MomentsService : WebService
{
    private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
    private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
    private static readonly IPublishInfo IpublishInfo = new PublishInfoDao();
    private static readonly AnonymousUserConfigSection anonymousUserConfigSection = (AnonymousUserConfigSection)WebConfigurationManager.GetSection("AnonymousUserConfigSection");

    public string UserID { get; private set; }
    public string UserName { get; private set; }
    public MomentsService()
    {

        string message = string.Empty;
        Cookie cookie = new Cookie();
        this.UserID = cookie.GetCookie("SNS_ID", out message);
        this.UserName = HttpUtility.UrlDecode(cookie.GetCookie("SNS_UserName", out message));
    }

    [WebMethod(BufferResponse = true, Description = "提交朋友圈发布信息")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(PublishInfo))]
    [GenerateScriptType(typeof(NoticeUserView))]
    [GenerateScriptType(typeof(AccessoryInfo))]
    [GenerateScriptType(typeof(SimpleAccessoryView))]
    public bool Submit(PublishInfo publishInfo, NoticeUserView noticeUserView, List<AccessoryInfo> publishAccessoryInfos, List<SimpleAccessoryView> removeSimpleAccessoryViews)
    {
        bool result = false;
        int accsCount = publishAccessoryInfos == null ? 0 : publishAccessoryInfos.Count;
        int removeSimpleAccessoryViewCount = removeSimpleAccessoryViews == null ? 0 : removeSimpleAccessoryViews.Count;
        PublishInfo tempInfo = ((ICommon<PublishInfo>)IpublishInfo).GetInformation(publishInfo.ID);
        if (tempInfo == null)
        {
            publishInfo.CreateTime = DateTime.Now;
            publishInfo.CreaterName = this.UserName;
            publishInfo.CreaterID = this.UserID;
            if (publishInfo.PublishType == ((int)PublishInfoTypeSetting.Short).ToString())
            {
                publishInfo.PublishTime = DateTime.Now;
                publishInfo.EditStatus = ((int)MiicYesNoSetting.No).ToString();
            }
            //---------------------------------------------新增朋友圈信息（单独，特殊情况）------------------------------------------------//
            if (accsCount == 0 && noticeUserView == null)
            {
                result = ((ICommon<PublishInfo>)IpublishInfo).Insert(publishInfo);
            }
            //---------------------------------------------新增朋友圈信息（不含附件只有提醒人）------------------------------------------------//
            else if (accsCount == 0 && noticeUserView != null)
            {
                result = IpublishInfo.Insert(publishInfo, noticeUserView);
            }
            //---------------------------------------------新增朋友圈信息（包含附件和提醒人）------------------------------------------------//
            else if (accsCount != 0)
            {
                result = IpublishInfo.Insert(publishInfo, noticeUserView, publishAccessoryInfos);
            }
        }
        else
        {
            //---------------------------------------------更新朋友圈信息（不包含附件和删除附件，且不含提醒人）------------------------------------------------//
            if (accsCount == 0 && noticeUserView == null && removeSimpleAccessoryViewCount == 0)
            {
                result = ((ICommon<PublishInfo>)IpublishInfo).Update(publishInfo);
            }
            //---------------------------------------------更新朋友圈信息（包含删除附件，不包含新增附件）------------------------------------------------//
            else if (accsCount == 0 && removeSimpleAccessoryViewCount != 0)
            {
                result = IpublishInfo.Update(publishInfo, removeSimpleAccessoryViews, noticeUserView);
            }
            //---------------------------------------------更新朋友圈信息（包含新增附件）------------------------------------------------//
            else if (accsCount != 0)
            {
                result = IpublishInfo.Update(publishInfo, publishAccessoryInfos, removeSimpleAccessoryViews, noticeUserView);
            }
        }
        return result;
    }

    [WebMethod(Description = "搜索我的朋友圈信息", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MyDateView))]
    [GenerateScriptType(typeof(MiicPage))]
    public string SearchMyMoments(MyDateView dateView, MiicPage page)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IpublishInfo.GetPersonMomentsPublishInfos(dateView, page);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       group dr by
                       new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID)].ToString(),
                           Title = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Title)].ToString(),
                           DetailContent = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Content)].ToString(),
                           Content = CommonService.DelImgStr(dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.PublishType)].ToString(), dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Content)].ToString(), false),
                           CreaterID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.CreaterID)].ToString(),
                           CreaterName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.CreaterName)].ToString(),
                           CreaterOrgName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.OrgName)].ToString(),
                           CreaterUserUrl = CommonService.GetManageFullUrl(dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.UserUrl)].ToString()),
                           CreaterUserType = dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.UserType)].ToString(),
                           CreateTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.CreateTime)],
                           PublishTime = (DateTime?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.PublishTime)],
                           PublishType = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.PublishType)].ToString(),
                           BrowseNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.BrowseNum)],
                           PraiseNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.PraiseNum)],
                           TreadNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.TreadNum)],
                           TransmitNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.TransmitNum)],
                           ReportNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.ReportNum)],
                           CommentNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.CommentNum)],
                           CollectNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.CollectNum)],
                           Remark = dr["REMARK"].ToString(),
                           RealName = dr["REAL_NAME"].ToString(),
                           IsFriend = dr["IS_FRIEND"].ToString()
                       } into g
                       select new
                       {
                           ID = g.Key.ID,
                           Title = g.Key.Title,
                           DetailContent = g.Key.DetailContent,
                           Content = g.Key.Content,
                           CreaterID = g.Key.CreaterID,
                           CreaterName = g.Key.CreaterName,
                           CreaterOrgName = g.Key.CreaterOrgName,
                           CreaterUserUrl = g.Key.CreaterUserUrl,
                           CreaterUserType = g.Key.CreaterUserType,
                           CreateTime = g.Key.CreateTime,
                           PublishTime = g.Key.PublishTime,
                           PublishType = g.Key.PublishType,
                           BrowseNum = g.Key.BrowseNum,
                           PraiseNum = g.Key.PraiseNum,
                           TreadNum = g.Key.TreadNum,
                           TransmitNum = g.Key.TransmitNum,
                           ReportNum = g.Key.ReportNum,
                           CommentNum = g.Key.CommentNum,
                           CollectNum = g.Key.CollectNum,
                           Remark = g.Key.Remark,
                           RealName = g.Key.RealName,
                           IsFriend = g.Key.IsFriend,
                           IsPraise = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.PraiseInfo, string>(u => u.PraiserID)]) == false && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.PraiseInfo, string>(u => u.PraiserID)].ToString() == UserID)) == 0 ? false : true,
                           IsTread = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.TreadInfo, string>(u => u.TreaderID)]) == false && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.TreadInfo, string>(u => u.TreaderID)].ToString() == UserID)) == 0 ? false : true,
                           IsReport = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.ReportInfo, string>(u => u.ReporterID)]) == false && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.ReportInfo, string>(u => u.ReporterID)].ToString() == UserID)) == 0 ? false : true,
                           IsCollect = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.CollectInfo, string>(u => u.CollectorID)]) == false
                                && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.CollectInfo, string>(u => u.CollectorID)].ToString() == UserID)
                                && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.CollectInfo, string>(u => u.CollectValid)].ToString() == ((int)MiicValidTypeSetting.Valid).ToString())) == 0 ? false : true,
                           AccInfos = (from item in g
                                       where Convert.IsDBNull(item["MomentsPublishAccessoryInfo" + Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID)]) == false
                                       select new
                                       {
                                           MonmentsPublishAccessoryInfoID = item["MomentsPublishAccessoryInfo" + Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID)].ToString(),
                                           FileName = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileName)].ToString(),
                                           FilePath = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FilePath)].ToString(),
                                           UploadTime = (DateTime?)item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, DateTime?>(o => o.UploadTime)],
                                           FileType = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileType)].ToString()
                                       }).Distinct()
                       };

            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
    [WebMethod(Description = "搜索我的朋友圈信息数", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MyDateView))]
    public int GetSearchMyMomentsCount(MyDateView dateView)
    {
        return IpublishInfo.GetPersonMomentsPublishCount(dateView);
    }

    [WebMethod(Description = "搜索好友的朋友圈信息", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(PersonDateView))]
    [GenerateScriptType(typeof(MiicPage))]
    public string SearchPersonMoments(PersonDateView dateView, MiicPage page)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IpublishInfo.GetPersonMomentsPublishInfos(dateView, page);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       group dr by
                       new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.ID)].ToString(),
                           Title = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.Title)].ToString(),
                           DetailContent = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.Content)].ToString(),
                           Content = CommonService.DelImgStr(dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.PublishType)].ToString(), dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.Content)].ToString(), false),
                           CreaterID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.CreaterID)].ToString(),
                           CreaterName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.CreaterName)].ToString(),
                           CreaterOrgName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.OrgName)].ToString(),
                           CreaterUserUrl = CommonService.GetManageFullUrl(dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.UserUrl)].ToString()),
                           CreaterUserType = dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.UserType)].ToString(),
                           CreateTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, DateTime?>(o => o.CreateTime)],
                           PublishTime = (DateTime?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, DateTime?>(o => o.PublishTime)],
                           PublishType = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.PublishType)].ToString(),
                           BrowseNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, int?>(o => o.BrowseNum)],
                           PraiseNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, int?>(o => o.PraiseNum)],
                           TreadNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, int?>(o => o.TreadNum)],
                           TransmitNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, int?>(o => o.TransmitNum)],
                           ReportNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, int?>(o => o.ReportNum)],
                           CommentNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, int?>(o => o.CommentNum)],
                           CollectNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, int?>(o => o.CollectNum)],
                           Remark = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.Remark)].ToString(),
                       } into g
                       select new
                       {
                           ID = g.Key.ID,
                           Title = g.Key.Title,
                           DetailContent = g.Key.DetailContent,
                           Content = g.Key.Content,
                           CreaterID = g.Key.CreaterID,
                           CreaterName = g.Key.CreaterName,
                           CreaterOrgName = g.Key.CreaterOrgName,
                           CreaterUserUrl = g.Key.CreaterUserUrl,
                           CreaterUserType = g.Key.CreaterUserType,
                           CreateTime = g.Key.CreateTime,
                           PublishTime = g.Key.PublishTime,
                           PublishType = g.Key.PublishType,
                           BrowseNum = g.Key.BrowseNum,
                           PraiseNum = g.Key.PraiseNum,
                           TreadNum = g.Key.TreadNum,
                           TransmitNum = g.Key.TransmitNum,
                           ReportNum = g.Key.ReportNum,
                           CommentNum = g.Key.CommentNum,
                           CollectNum = g.Key.CollectNum,
                           Remark = g.Key.Remark,
                           IsPraise = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.PraiseInfo, string>(u => u.PraiserID)]) == false && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.PraiseInfo, string>(u => u.PraiserID)].ToString() == UserID)) == 0 ? false : true,
                           IsTread = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.TreadInfo, string>(u => u.TreaderID)]) == false && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.TreadInfo, string>(u => u.TreaderID)].ToString() == UserID)) == 0 ? false : true,
                           IsReport = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.ReportInfo, string>(u => u.ReporterID)]) == false && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.ReportInfo, string>(u => u.ReporterID)].ToString() == UserID)) == 0 ? false : true,
                           IsCollect = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.CollectInfo, string>(u => u.CollectorID)]) == false
                                && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.CollectInfo, string>(u => u.CollectorID)].ToString() == UserID)
                                && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.CollectInfo, string>(u => u.CollectValid)].ToString() == ((int)MiicValidTypeSetting.Valid).ToString())) == 0 ? false : true,
                           AccInfos = (from item in g
                                       where Convert.IsDBNull(item["MomentsPublishAccessoryInfo" + Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID)]) == false
                                       select new
                                       {
                                           MonmentsPublishAccessoryInfoID = item["MomentsPublishAccessoryInfo" + Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID)].ToString(),
                                           FileName = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileName)].ToString(),
                                           FilePath = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FilePath)].ToString(),
                                           UploadTime = (DateTime?)item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, DateTime?>(o => o.UploadTime)],
                                           FileType = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileType)].ToString()
                                       }).Distinct()
                       };

            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
    [WebMethod(Description = "搜索好友的朋友圈信息数", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(PersonDateView))]
    public int GetSearchPersonMomentsCount(PersonDateView dateView)
    {
        return IpublishInfo.GetPersonMomentsPublishCount(dateView);
    }

    [WebMethod(BufferResponse = true, Description = "删除临时提交文件")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool DeleteTempFile(string tempPath)
    {
        if (string.IsNullOrEmpty(tempPath))
        {
            throw new ArgumentNullException("tempPath", "参数tempPath:不能为空");
        }
        bool result = false;
        try
        {
            File.Delete(Server.MapPath(tempPath));
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

    [WebMethod(BufferResponse = true, Description = "删除临时提交文件(多个)")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool DeleteTempFiles(List<string> tempPaths)
    {
        if (tempPaths.Count == 0)
        {
            throw new ArgumentNullException("tempPath", "参数tempPath:不能为空");
        }
        bool result = false;
        try
        {
            foreach (var item in tempPaths)
            {
                File.Delete(Server.MapPath(item));
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

    [WebMethod(Description = "获取朋友圈信息的年份列表", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MyDateView))]
    public List<string> GetYears(MyDateView dateView)
    {
        return IpublishInfo.GetPersonMomentsPublishInfosYearList(dateView);
    }

    [WebMethod(Description = "获取朋友圈信息的月份列表", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MyDateView))]
    public List<string> GetMonths(MyDateView dateView)
    {
        return IpublishInfo.GetPersonMomentsPublishInfosMonthList(dateView);
    }

    [WebMethod(Description = "获取某人信息的年份列表", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(PersonDateView))]
    public List<string> GetPersonYears(PersonDateView dateView)
    {
        return IpublishInfo.GetPersonMomentsPublishInfosYearList(dateView);
    }

    [WebMethod(Description = "获取某人信息的月份列表", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(PersonDateView))]
    public List<string> GetPersonMonths(PersonDateView dateView)
    {
        return IpublishInfo.GetPersonMomentsPublishInfosMonthList(dateView);
    }

    [WebMethod(BufferResponse = true, Description = "获取我的最新发布的朋友圈信息列表（条件：长篇、已发布的、有效的、上线的）")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetMyPublishInfos(int top)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IpublishInfo.GetTopSimpleMomentsInfos(this.UserID, top);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID)].ToString(),
                           Title = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Title)].ToString(),
                           Content = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Content)].ToString(),
                           CreaterID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.CreaterID)].ToString(),
                           CreaterName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.CreaterName)].ToString(),
                           CreateTime = (DateTime?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.CreateTime)],
                           CreaterUserUrl = CommonService.GetManageFullUrl(dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.MiicSocialUserInfo, string>(o => o.MicroUserUrl)].ToString()),
                           PublishTime = (DateTime?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.PublishTime)],
                           MicroType = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.PublishType)].ToString(),
                           BrowseNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.BrowseNum)],
                           PraiseNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.PraiseNum)],
                           TreadNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.TreadNum)],
                           TransmitNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.TransmitNum)],
                           CollectNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.CollectNum)]
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }


    [WebMethod(Description = "搜索我的（含主页和个人页）最新的朋友圈信息", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MyTopView))]
    public string GetMyNewestMoments(MyTopView topView)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IpublishInfo.GetNewestMomentsPublishInfos(topView);
        if (dt.Rows.Count > 0)
        {
            result = GetPublishListString(dt);
        }
        return result;
    }


    [WebMethod(Description = "搜索他人（个人页）最新的朋友圈信息", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(PersonTopView))]
    public string GetPersonNewestMoments(PersonTopView topView)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IpublishInfo.GetNewestMomentsPublishInfos(topView);
        if (dt.Rows.Count > 0)
        {
            result = GetPublishListString(dt);
        }
        return result;
    }

    [WebMethod(Description = "搜索我的（含主页和个人页）最旧的朋友圈信息", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MyTopView))]
    public string GetMyOldestMoments(MyTopView topView)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IpublishInfo.GetOldestMomentsPubilishInfos(topView);
        if (dt.Rows.Count > 0)
        {
            result = GetPublishListString(dt);
        }
        return result;
    }

    [WebMethod(Description = "搜索他人的（个人页）最旧的朋友圈信息", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(PersonTopView))]
    public string GetPersonOldestMoments(PersonTopView topView)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IpublishInfo.GetOldestMomentsPubilishInfos(topView);
        if (dt.Rows.Count > 0)
        {
            result = GetPublishListString(dt);
        }
        return result;
    }

    private string GetPublishListString(DataTable dt)
    {
        string result = CommonService.InitialJsonList;
        var temp = from dr in dt.AsEnumerable()
                   group dr by
                   new
                   {
                       ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.ID)].ToString(),
                       Title = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.Title)].ToString(),
                       DetailContent = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Content)].ToString(),
                       Content = CommonService.DelImgStr(dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.PublishType)].ToString(), dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.Content)].ToString(), false),
                       CreaterID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.CreaterID)].ToString(),
                       CreaterName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.CreaterName)].ToString(),
                       CreaterOrgName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.OrgName)].ToString(),
                       CreaterUserUrl = CommonService.GetManageFullUrl(dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.UserUrl)].ToString()),
                       CreaterUserType = dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.UserType)].ToString(),
                       CreateTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, DateTime?>(o => o.CreateTime)],
                       PublishTime = (DateTime?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, DateTime?>(o => o.PublishTime)],
                       PublishType = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.PublishType)].ToString(),
                       BrowseNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, int?>(o => o.BrowseNum)],
                       PraiseNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, int?>(o => o.PraiseNum)],
                       TreadNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, int?>(o => o.TreadNum)],
                       TransmitNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, int?>(o => o.TransmitNum)],
                       ReportNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, int?>(o => o.ReportNum)],
                       CommentNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, int?>(o => o.CommentNum)],
                       CollectNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, int?>(o => o.CollectNum)],
                       Remark = dr[Config.Attribute.GetSqlColumnNameByPropertyName<AddressPublishInfo, string>(o => o.Remark)].ToString(),
                   } into g
                   select new
                   {
                       ID = g.Key.ID,
                       Title = g.Key.Title,
                       DetailContent = g.Key.DetailContent,
                       Content = g.Key.Content,
                       CreaterID = g.Key.CreaterID,
                       CreaterName = g.Key.CreaterName,
                       CreaterOrgName = g.Key.CreaterOrgName,
                       CreaterUserUrl = g.Key.CreaterUserUrl,
                       CreaterUserType = g.Key.CreaterUserType,
                       CreateTime = g.Key.CreateTime,
                       PublishTime = g.Key.PublishTime,
                       PublishType = g.Key.PublishType,
                       BrowseNum = g.Key.BrowseNum,
                       PraiseNum = g.Key.PraiseNum,
                       TreadNum = g.Key.TreadNum,
                       TransmitNum = g.Key.TransmitNum,
                       ReportNum = g.Key.ReportNum,
                       CommentNum = g.Key.CommentNum,
                       CollectNum = g.Key.CollectNum,
                       Remark = g.Key.Remark,
                       IsPraise = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.PraiseInfo, string>(u => u.PraiserID)]) == false && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.PraiseInfo, string>(u => u.PraiserID)].ToString() == UserID)) == 0 ? false : true,
                       IsTread = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.TreadInfo, string>(u => u.TreaderID)]) == false && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.TreadInfo, string>(u => u.TreaderID)].ToString() == UserID)) == 0 ? false : true,
                       IsReport = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.ReportInfo, string>(u => u.ReporterID)]) == false && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.ReportInfo, string>(u => u.ReporterID)].ToString() == UserID)) == 0 ? false : true,
                       IsCollect = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.CollectInfo, string>(u => u.CollectorID)]) == false
                            && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.CollectInfo, string>(u => u.CollectorID)].ToString() == UserID)
                            && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Moments.Behavior.CollectInfo, string>(u => u.CollectValid)].ToString() == ((int)MiicValidTypeSetting.Valid).ToString())) == 0 ? false : true,
                       AccInfos = (from item in g
                                   where Convert.IsDBNull(item["MomentsPublishAccessoryInfo" + Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID)]) == false
                                   select new
                                   {
                                       ID = item["MomentsPublishAccessoryInfo" + Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID)].ToString(),
                                       FileName = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileName)].ToString(),
                                       FilePath = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FilePath)].ToString(),
                                       UploadTime = (DateTime?)item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, DateTime?>(o => o.UploadTime)],
                                       FileType = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileType)].ToString()
                                   }).Distinct()
                   };

        result = Config.Serializer.Serialize(temp);
        return result;
    }

    [WebMethod(BufferResponse = true, Description = "删除朋友圈发布信息")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool Delete(string publishID)
    {
        if (string.IsNullOrEmpty(publishID))
        {
            throw new ArgumentNullException("publishID", "参数publishID:不能为空");
        }
        return ((ICommon<PublishInfo>)IpublishInfo).Delete(publishID);
    }

    [WebMethod(BufferResponse = true, Description = "获取我的草稿箱信息列表")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(DraftSearchView))]
    [GenerateScriptType(typeof(MiicPage))]
    public string GetDraftInfos(DraftSearchView keywordView, MiicPage page)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IpublishInfo.GetDraftInfos(keywordView, page);
        if (dt.Rows.Count > 0)
        {
            var temp = (from dr in dt.AsEnumerable()
                        group dr by
                        new
                        {
                            ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID)].ToString(),
                            Title = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Title)].ToString(),
                            DetailContent = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Content)].ToString(),
                            Content = CommonService.DelImgStr(dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.PublishType)].ToString(), dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Content)].ToString(), true),
                            CreaterID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.CreaterID)].ToString(),
                            CreaterName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.CreaterName)].ToString(),
                            CreaterOrgName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.OrgName)].ToString(),
                            CreaterUserUrl = CommonService.GetManageFullUrl(dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.UserUrl)].ToString()),
                            CreaterUserType = dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.UserType)].ToString(),
                            CreateTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.CreateTime)],
                            PublishTime = Convert.IsDBNull(dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.PublishTime)]) ? null : (DateTime?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.PublishTime)],
                            HasAcc = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.HasAcc)].ToString(),
                            PublishType = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.PublishType)].ToString(),
                            EditStatus = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.EditStatus)].ToString(),
                            UpdateTime = Convert.IsDBNull(dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.UpdateTime)]) ? null : (DateTime?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.UpdateTime)]
                        } into g
                        select new
                        {
                            ID = g.Key.ID,
                            Title = g.Key.Title,
                            DetailContent = g.Key.DetailContent,
                            Content = g.Key.Content,
                            CreaterID = g.Key.CreaterID,
                            CreaterName = g.Key.CreaterName,
                            CreaterOrgName = g.Key.CreaterOrgName,
                            CreaterUserUrl = g.Key.CreaterUserUrl,
                            CreaterUserType = g.Key.CreaterUserType,
                            CreateTime = g.Key.CreateTime,
                            PublishTime = g.Key.PublishTime,
                            HasAcc = g.Key.HasAcc,
                            PublishType = g.Key.PublishType,
                            EditStatus = g.Key.EditStatus,
                            UpdateTime = g.Key.UpdateTime,
                            AccInfos = (from item in g.AsEnumerable()
                                        where Convert.IsDBNull(item["MomentsAccessoryInfo" + Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID)]) == false
                                        select new
                                        {
                                            ID = item["MomentsAccessoryInfo" + Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID)].ToString(),
                                            FileName = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileName)].ToString(),
                                            FilePath = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FilePath)].ToString(),
                                            UploadTime = (DateTime?)item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, DateTime?>(o => o.UploadTime)],
                                            FileType = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileType)].ToString()
                                        }).Distinct()
                        });

            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }

    [WebMethod(BufferResponse = true, Description = "获取我的草稿箱信息数")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(DraftSearchView))]
    public int GetDraftInfoCount(DraftSearchView keywordView)
    {
        return IpublishInfo.GetDraftInfoCount(keywordView);
    }

    [WebMethod(BufferResponse = true, Description = "设置草稿直接发布")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool SetPublish(string publishID)
    {
        if (string.IsNullOrEmpty(publishID))
        {
            throw new ArgumentNullException("publishID", "参数publishID:不能为空");
        }

        bool result = false;

        result = ((ICommon<PublishInfo>)IpublishInfo).Update(new PublishInfo()
        {
            ID = publishID,
            PublishTime = DateTime.Now,
            EditStatus = ((int)MiicYesNoSetting.No).ToString()
        });

        return result;
    }

    [WebMethod(BufferResponse = true, Description = "获取详细信息")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetDetailInformation(string publishID)
    {
        if (string.IsNullOrEmpty(publishID))
        {
            throw new ArgumentNullException("publishID", "参数publishID:不能为空");
        }
        string result = CommonService.InitialJsonList;
        TransactionOptions tranOptions = new TransactionOptions();
        tranOptions.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, tranOptions))
        {
            PublishInfo info = ((ICommon<PublishInfo>)IpublishInfo).GetInformation(publishID);
            List<AccessoryInfo> accList = null;
            if (info.HasAcc == ((int)MiicYesNoSetting.Yes).ToString())
            {
                accList = ((PublishInfoDao)IpublishInfo).GetAccessoryList(publishID);
            }
            MyBehaviorView behaviorView = new MyBehaviorView()
            {
                PublishID = publishID
            };
            DataTable behaviorFlagDt = IpublishInfo.GetMyMomentsBehaviorFlags(behaviorView);
            bool isPraise = (behaviorFlagDt.Rows[0]["PraiseFlag"].ToString() == ((int)MiicYesNoSetting.Yes).ToString()) ? true : false;
            bool isTread = (behaviorFlagDt.Rows[0]["TreadFlag"].ToString() == ((int)MiicYesNoSetting.Yes).ToString()) ? true : false;
            bool isReport = (behaviorFlagDt.Rows[0]["ReportFlag"].ToString() == ((int)MiicYesNoSetting.Yes).ToString()) ? true : false;
            bool isCollect = (behaviorFlagDt.Rows[0]["CollectFlag"].ToString() == ((int)MiicYesNoSetting.Yes).ToString()) ? true : false;
            ts.Complete();
            result = Config.Serializer.Serialize(new
            {
                PublishInfo = info,
                IsPraise = isPraise,
                IsTread = isTread,
                IsReport = isReport,
                IsCollect = isCollect,
                AccList = accList
            });
        }
        return result;
    }
}
