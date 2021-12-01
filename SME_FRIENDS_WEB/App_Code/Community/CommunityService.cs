using Miic.Base;
using Miic.Base.Setting;
using Miic.BaseStruct;
using Miic.DB.SqlObject;
using Miic.Friends.Common;
using Miic.Friends.Common.Setting;
using Miic.Friends.Community; 
using Miic.Friends.Notice;
using Miic.Friends.SimpleGroup;
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
/// 行业圈子服务
/// </summary>
[WebService(Namespace = "http://pyq.mictalk.cn/")]
[WebServiceBinding(ConformsTo = WsiProfiles.None)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[ScriptService]
public partial class CommunityService : WebService
{
    private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
    private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
    private static readonly ICommunityInfo IcommunityInfo = new CommunityInfoDao();
    private static readonly Miic.Friends.Group.IGroupInfo IgroupInfo = new Miic.Friends.Group.GroupInfoDao();
    private static readonly IPublishInfo IpublishInfo = new PublishInfoDao();
    public string UserID { get; private set; }
    public string UserName { get; private set; }
    public CommunityService()
    {

        string message = string.Empty;
        Cookie cookie = new Cookie();
        this.UserID = cookie.GetCookie("SNS_ID", out message);
        this.UserName = HttpUtility.UrlDecode(cookie.GetCookie("SNS_UserName", out message));
    }

    [WebMethod(BufferResponse = true, Description = "提交行业圈子发布信息")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(PublishInfo))]
    [GenerateScriptType(typeof(NoticeUserView))]
    [GenerateScriptType(typeof(AccessoryInfo))]
    [GenerateScriptType(typeof(SimpleLabelView))]
    [GenerateScriptType(typeof(SimpleAccessoryView))]
    public bool Submit(PublishInfo publishInfo, NoticeUserView noticeUserView, List<AccessoryInfo> publishAccessoryInfos, List<SimpleLabelView> simpleLabelViews, List<SimpleAccessoryView> removeSimpleAccessoryViews, List<string> removeSimpleLabelViewIDs)
    {
        bool result = false;
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
            result = IpublishInfo.Insert(publishInfo, publishAccessoryInfos, simpleLabelViews, noticeUserView);

        }
        else
        {
            result = IpublishInfo.Update(publishInfo, publishAccessoryInfos, simpleLabelViews, removeSimpleAccessoryViews, removeSimpleLabelViewIDs, noticeUserView);
        }
        return result;
    }

    [WebMethod(BufferResponse = true, Description = "删除行业圈子发布信息")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool Delete(string publishID)
    {
        if (string.IsNullOrEmpty(publishID))
        {
            throw new ArgumentNullException("publishID", "参数publishID:不能为空");
        }
        return ((ICommon<PublishInfo>)IpublishInfo).Delete(publishID);
    }


    [WebMethod(Description = "展示我的行业圈子列表（右侧展示）", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string ShowMyCommunityInfoList(int top)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IcommunityInfo.GetCommunityInfoList(this.UserID, top);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.ID)],
                           Name = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.Name)],
                           LogoUrl = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.LogoUrl)]
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }

    [WebMethod(Description = "展示行业圈子推荐列表（右侧展示）", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string ShowCommunityRecommendInfoList(int top)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IcommunityInfo.GetHotCommunityRecommendInfoList(top);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.ID)],
                           Name = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.Name)],
                           LogoUrl = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.LogoUrl)]
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }

    [WebMethod(Description = "创建行业圈子,可附带邀请人列表", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(CommunityInfo))]
    [GenerateScriptType(typeof(CommunityApplicationInfo))]
    public bool Add(CommunityInfo communityInfo, List<CommunityApplicationInfo> memberApplications)
    {
        bool result = false;
        communityInfo.CreaterID = this.UserID;
        communityInfo.CreaterName = this.UserName;
        communityInfo.CreateTime = DateTime.Now;
        communityInfo.Valid = ((int)MiicValidTypeSetting.Valid).ToString();
        communityInfo.MemberCount = 1;
        if (memberApplications == null || memberApplications.Count == 0)
        {
            result = IcommunityInfo.Insert(communityInfo);
        }
        else
        {
            result = IcommunityInfo.Insert(communityInfo, memberApplications);
        }
        return result;
    }

    [WebMethod(Description = "移除行业圈子", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool Remove(string communityID)
    {
        return ((ICommon<CommunityInfo>)IcommunityInfo).Delete(communityID);
    }
    [WebMethod(Description = "搜索我的行业圈子", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MySimpleGroupSearchView))]
    [GenerateScriptType(typeof(MiicPage))]
    public string Search(MySimpleGroupSearchView searchView, MiicPage page)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IcommunityInfo.Search(searchView, page);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, string>(o => o.ID)],
                           Name = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, string>(o => o.Name)],
                           LogoUrl = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, string>(o => o.LogoUrl)],
                           MemberCount = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, int?>(o => o.MemberCount)],
                           CreaterID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, string>(o => o.CreaterID)],
                           CreaterName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, string>(o => o.CreaterName)],
                           CreateTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, DateTime?>(o => o.CreateTime)],
                           JoinTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, DateTime?>(o => o.JoinTime)],
                           Remark = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, string>(o => o.Remark)].ToString(),
                           TopicCount = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, int?>(o => o.TopicCount)],
                           MessageCount = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, int?>(o => o.MessageCount)],
                           IsAdmin = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, string>(o => o.IsAdmin)],
                           CommunityMemberID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, string>(o => o.CommunityMemberID)]
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }

    [WebMethod(Description = "搜索我的行业圈子数", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MySimpleGroupSearchView))]
    public int GetSearchCount(MySimpleGroupSearchView searchView)
    {
        return IcommunityInfo.GetSearchCount(searchView);
    }
    [WebMethod(MessageName = "RecommendSearch", Description = "搜索热门行业圈子", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MyKeywordView))]
    [GenerateScriptType(typeof(MiicPage))]
    public string Search(MyKeywordView keywordView, MiicPage page)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IcommunityInfo.SearchHotCommunity(keywordView, page);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, string>(o => o.ID)],
                           Name = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, string>(o => o.Name)],
                           LogoUrl = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, string>(o => o.LogoUrl)],
                           MemberCount = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, int?>(o => o.MemberCount)],
                           CreaterID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, string>(o => o.CreaterID)],
                           CreaterName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, string>(o => o.CreaterName)],
                           CreateTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, DateTime?>(o => o.CreateTime)],
                           JoinTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, DateTime?>(o => o.JoinTime)],
                           Remark = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, string>(o => o.Remark)].ToString(),
                           TopicCount = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, int?>(o => o.TopicCount)],
                           MessageCount = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, int?>(o => o.MessageCount)]

                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
    [WebMethod(MessageName = "GetRecommendSearchCount", Description = "搜索热门行业圈子数", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(Miic.Friends.Common.NoPersonKeywordView))]
    public int GetSearchCount(Miic.Friends.Common.NoPersonKeywordView keywordView)
    {
        return IcommunityInfo.GetSearchHotCommunityCount(keywordView);
    }
    [WebMethod(Description = "获取行业圈子基本信息", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public CommunityInfo GetCommunityInfo(string id)
    {
        return ((ICommon<CommunityInfo>)IcommunityInfo).GetInformation(id);
    }
    [WebMethod(Description = "修改行业圈子基本信息，以及设置成员", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool Update(CommunityInfo communityInfo, List<CommunityMember> member)
    {
        bool result = false;
        if (member == null)
        {
            result = ((ICommon<CommunityInfo>)IcommunityInfo).Update(communityInfo);
        }
        else
        {
            result = IcommunityInfo.Update(communityInfo, member);
        }
        return result;
    }
    [WebMethod(Description = "搜索我的最新动态行业圈子", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MySimpleGroupSearchView))]
    [GenerateScriptType(typeof(MiicPage))]
    public string TrendsSearch(MySimpleGroupSearchView searchView, MiicPage page)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IcommunityInfo.PersonTrendsSearch(searchView, page);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       group dr by new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.ID)],
                           Name = dr["NAME"],
                           LogoUrl = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.LogoUrl)],
                           MemberCount = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, int?>(o => o.MemberCount)],
                           Manager = dr["COMMUNITY_MANAGER"]
                       } into g
                       select new
                       {

                           ID = g.Key.ID,
                           Name = g.Key.Name,
                           MemberCount = g.Key.MemberCount,
                           LogoUrl = g.Key.LogoUrl,
                           Manager = g.Key.Manager,
                           TopicCount = (from item in g.AsEnumerable()
                                         where item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.Valid)].ToString() == ((int)MiicValidTypeSetting.Valid).ToString()
                                         select new
                                         {
                                             Content = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.TopicContent)],
                                             CreaterID = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.CreaterID)],
                                             CreaterName = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.CreaterName)],
                                             CreaterUrl = CommonService.GetManageFullUrl(item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.CreaterUrl)].ToString()),
                                             CreateTime = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, DateTime?>(o => o.CreateTime)],
                                             MessageCount = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, int?>(o => o.MessageCount)]
                                         }).Distinct().Count(),
                           TopicInfoList = g.Count(o => (Convert.IsDBNull(o["TOPIC_ID"]) == false && (o[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(x => x.Valid)].ToString() == ((int)MiicValidTypeSetting.Valid).ToString()))) == 0 ?
                                   null : (from item in g.AsEnumerable()
                                           where item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.Valid)].ToString() == ((int)MiicValidTypeSetting.Valid).ToString()
                                           select new
                                           {
                                               Content = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.TopicContent)],
                                               CreaterID = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.CreaterID)],
                                               CreaterName = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.CreaterName)],
                                               CreaterUrl = CommonService.GetManageFullUrl(item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.CreaterUrl)].ToString()),
                                               CreateTime = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, DateTime?>(o => o.CreateTime)],
                                               IsFriend = item["IS_FRIEND"].ToString(),
                                               MessageCount = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, int?>(o => o.MessageCount)]
                                           }).Distinct().Take(10),
                             LabelCount = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(u => u.LabelID)]) == false),
                           //LabelCount = (from item in g.AsEnumerable()
                           //              select new
                           //              {
                           //                  LabelID = item[Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelID)],
                           //                  LabelName = item[Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelName)]
                           //              }).Distinct().Count(),
                           LabelInfoList = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(u => u.LabelID)]) == false) == 0 ?
                                   null :
                                   (from item in g.AsEnumerable()
                                    select new
                                    {
                                        LabelID = item[Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelID)],
                                        LabelName = item[Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelName)]
                                    }).Distinct().Take(10)
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }

   

    [WebMethod(Description = "获取搜索我的最新动态行业圈子数", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MySimpleGroupSearchView))]
    public int GetTrendsSearchCount(MySimpleGroupSearchView searchView)
    {
        return IcommunityInfo.GetPersonTrendsSearchCount(searchView);
    }
    [WebMethod(Description = "展示我的行业圈子列表（右侧展示）(含具体标签信息)", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string ShowMyCommunityInfoDetailList(int top)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IcommunityInfo.PersonTrendsSearch(new MySimpleGroupSearchView()
        {
            Keyword = string.Empty
        }, new MiicPage()
        {
            pageStart = "1",
            pageEnd = top.ToString()
        });
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       group dr by new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.ID)],
                           Name = dr["NAME"],
                           LogoUrl = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.LogoUrl)],
                           MemberCount = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, int?>(o => o.MemberCount)]
                       } into g
                       select new
                       {

                           ID = g.Key.ID,
                           Name = g.Key.Name,
                           MemberCount = g.Key.MemberCount,
                           LogoUrl = g.Key.LogoUrl,
                           TopicCount = (from item in g.AsEnumerable()
                                         select new
                                         {
                                             Content = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.TopicContent)],
                                             CreaterID = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.CreaterID)],
                                             CreaterName = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.CreaterName)],
                                             CreaterUrl = CommonService.GetManageFullUrl(item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.CreaterUrl)].ToString()),
                                             CreateTime = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, DateTime?>(o => o.CreateTime)],
                                             MessageCount = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, int?>(o => o.MessageCount)]
                                         }).Distinct().Count(),
                           TopicInfoList = g.Count(o => Convert.IsDBNull(o["TOPIC_ID"]) == false) == 0 ?
                                   null : (from item in g.AsEnumerable()
                                           select new
                                           {
                                               Content = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.TopicContent)],
                                               CreaterID = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.CreaterID)],
                                               CreaterName = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.CreaterName)],
                                               CreaterUrl = CommonService.GetManageFullUrl(item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.CreaterUrl)].ToString()),
                                               CreateTime = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, DateTime?>(o => o.CreateTime)],
                                               MessageCount = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, int?>(o => o.MessageCount)]
                                           }).Distinct().Take(10),
                           LabelCount = (from item in g.AsEnumerable()
                                         select new
                                         {
                                             LabelID = item[Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelID)],
                                             LabelName = item[Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelName)]
                                         }).Distinct().Count(),
                           LabelInfoList = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(u => u.LabelID)]) == false) == 0 ?
                                   null :
                                   (from item in g.AsEnumerable()
                                    select new
                                    {
                                        LabelID = item[Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelID)],
                                        LabelName = item[Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelName)]
                                    }).Distinct().Take(10)
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }

    [WebMethod(Description = "判断是否有行业圈子", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool HaveCommunity()
    {
        bool result = IcommunityInfo.HaveCommunity(this.UserID); 
        return result;
    }
    [WebMethod(Description = "判断是否有行业圈子和讨论组", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string HaveCommunityAndGroup()
    {
        string result = CommonService.InitialJsonList;
        bool communityResult = IcommunityInfo.HaveCommunity(this.UserID);
        bool groupResult = IgroupInfo.HaveGroup(this.UserID);
        result = Config.Serializer.Serialize(new { CommunityResult = communityResult, GroupResult = groupResult });
        return result;
    }
    
    [WebMethod(Description = "搜索行业圈信息", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(CommunityDateView))]
    [GenerateScriptType(typeof(MiicPage))]
    public string GetCommunityPublishInfos(CommunityDateView dateView, MiicPage page)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IpublishInfo.GetCommunityPublishInfos(dateView, page);
        if (dt.Rows.Count > 0)
        {
            result = GetPublishListString(dt);
        }
        return result;
    }
    [WebMethod(Description = "获取搜索行业圈信息数", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(CommunityDateView))]
    public int GetCommunityPublishInfoCount(CommunityDateView dateView)
    {
        return IpublishInfo.GetCommunityPublishCount(dateView);
    }

    [WebMethod(Description = "获取年份", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(CommunityDateView))]
    public List<string> GetYears(CommunityDateView dateView)
    {
        return IpublishInfo.GetCommunityPublishInfosYearList(dateView);
    }

    [WebMethod(Description = "获取月份", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(CommunityDateView))]
    public List<string> GetMonths(CommunityDateView dateView)
    {
        return IpublishInfo.GetCommunityPublishInfosMonthList(dateView);
    }

    [WebMethod(Description = "搜索最旧的行业圈信息", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(CommunityTopView))]
    public string GetOldestCommunityPublishInfo(CommunityTopView topView)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IpublishInfo.GetOldestCommunityPubilishInfos(topView);
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
                       ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, string>(o => o.ID)].ToString(),
                       CommunityID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.CommunityID)],
                       LabelID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelID)],
                       LabelName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelName)],
                       Title = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, string>(o => o.Title)].ToString(),
                       DetailContent = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, string>(o => o.Content)].ToString(),
                       Content = CommonService.DelImgStr(dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, string>(o => o.PublishType)].ToString(), dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, string>(o => o.Content)].ToString(), false),
                       CreaterID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, string>(o => o.CreaterID)].ToString(),
                       CreaterName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, string>(o => o.CreaterName)].ToString(),
                       CreaterOrgName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, string>(o => o.OrgName)].ToString(),
                       CreaterUserUrl = CommonService.GetManageFullUrl(dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, string>(o => o.MicroUserUrl)].ToString()),
                       CreaterUserType = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, string>(o => o.UserType)].ToString(),
                       CreateTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, DateTime?>(o => o.CreateTime)],
                       PublishTime = (DateTime?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, DateTime?>(o => o.PublishTime)],
                       PublishType = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, string>(o => o.PublishType)].ToString(),
                       BrowseNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, int?>(o => o.BrowseNum)],
                       PraiseNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, int?>(o => o.PraiseNum)],
                       TreadNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, int?>(o => o.TreadNum)],
                       TransmitNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, int?>(o => o.TransmitNum)],
                       ReportNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, int?>(o => o.ReportNum)],
                       CommentNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, int?>(o => o.CommentNum)],
                       CollectNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, int?>(o => o.CollectNum)],
                       Remark = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, string>(o => o.UserName)].ToString(),
                       IsFriend = dr["IS_FRIEND"].ToString()
                   } into g
                   select new
                   {
                       ID = g.Key.ID,
                       CommunityID = g.Key.CommunityID,
                       LabelID = g.Key.LabelID,
                       LabelName = g.Key.LabelName,
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
                       IsFriend = g.Key.IsFriend,
                       IsPraise = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Community.Behavior.PraiseInfo, string>(u => u.PraiserID)]) == false && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Community.Behavior.PraiseInfo, string>(u => u.PraiserID)].ToString() == UserID)) == 0 ? false : true,
                       IsTread = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Community.Behavior.TreadInfo, string>(u => u.TreaderID)]) == false && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Community.Behavior.TreadInfo, string>(u => u.TreaderID)].ToString() == UserID)) == 0 ? false : true,
                       IsReport = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Community.Behavior.ReportInfo, string>(u => u.ReporterID)]) == false && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Community.Behavior.ReportInfo, string>(u => u.ReporterID)].ToString() == UserID)) == 0 ? false : true,
                       IsCollect = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Community.Behavior.CollectInfo, string>(u => u.CollectorID)]) == false
                            && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Community.Behavior.CollectInfo, string>(u => u.CollectorID)].ToString() == UserID)
                            && (o[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Community.Behavior.CollectInfo, string>(u => u.CollectValid)].ToString() == ((int)MiicValidTypeSetting.Valid).ToString())) == 0 ? false : true,
                       AccInfos = (from item in g
                                   where Convert.IsDBNull(item["CommunityPublishAccessoryInfo" + Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID)]) == false
                                   select new
                                   {
                                       MonmentsPublishAccessoryInfoID = item["CommunityPublishAccessoryInfo" + Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID)].ToString(),
                                       FileName = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileName)].ToString(),
                                       FilePath = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FilePath)].ToString(),
                                       UploadTime = (DateTime?)item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, DateTime?>(o => o.UploadTime)],
                                       FileType = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileType)].ToString()
                                   }).Distinct()
                   };

        result = Config.Serializer.Serialize(temp);
        return result;
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
                                        where Convert.IsDBNull(item["CommunityAccessoryInfo" + Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID)]) == false
                                        select new
                                        {
                                            ID = item["CommunityAccessoryInfo" + Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID)].ToString(),
                                            FileName = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileName)].ToString(),
                                            FilePath = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FilePath)].ToString(),
                                            UploadTime = (DateTime?)item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, DateTime?>(o => o.UploadTime)],
                                            FileType = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileType)].ToString()
                                        }).Distinct(),
                            Community = (from item in g.AsEnumerable()
                                         group item by
                                         new
                                         {
                                             CommunityID = item[Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.CommunityID)].ToString()
                                         } into k
                                         select new {
                                             CommunityID = k.Key.CommunityID,
                                             LabelInfos = (from item in k.AsEnumerable()
                                                           select new
                                                           {
                                                               LabelID = item[Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelID)].ToString(),
                                                               LabelName = item[Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelName)].ToString()
                                                           }).Distinct()
                                         })
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

    [WebMethod(BufferResponse = true, Description = "获取发布详细信息（基本信息、附件）")]
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
            if (info != null)
            {
                if (info.HasAcc == ((int)MiicYesNoSetting.Yes).ToString())
                {
                    List<AccessoryInfo> accs = ((PublishInfoDao)IpublishInfo).GetAccessoryList(publishID);
                    accList = new List<AccessoryInfo>();
                    foreach (var acc in accs.AsEnumerable())
                    {
                        accList.Add(new AccessoryInfo()
                        {
                            ID = acc.ID,
                            PublishID = acc.PublishID,
                            FileName = acc.FileName,
                            FilePath = acc.FilePath,
                            UploadTime = acc.UploadTime,
                            FileType = acc.FileType
                        });
                    }
                }
                MyCommunityBehaviorView behaviorView = new MyCommunityBehaviorView()
                {
                    PublishID = publishID
                };
                DataTable behaviorFlagDt = IpublishInfo.GetMyCommunityBehaviorFlags(behaviorView);
                bool isPraise = (behaviorFlagDt.Rows[0]["PraiseFlag"].ToString() == ((int)MiicYesNoSetting.Yes).ToString()) ? true : false;
                bool isTread = (behaviorFlagDt.Rows[0]["TreadFlag"].ToString() == ((int)MiicYesNoSetting.Yes).ToString()) ? true : false;
                bool isReport = (behaviorFlagDt.Rows[0]["ReportFlag"].ToString() == ((int)MiicYesNoSetting.Yes).ToString()) ? true : false;
                bool isCollect = (behaviorFlagDt.Rows[0]["CollectFlag"].ToString() == ((int)MiicYesNoSetting.Yes).ToString()) ? true : false;
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
            ts.Complete();
        }
        return result;
    }
    [WebMethod(Description = "获取我的通讯录表（不在该行业圈子内）", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetMyAddressBookList(string communityID)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IcommunityInfo.GetPersonAddressBookList(communityID, this.UserID);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           AddresserID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.AddressBook.AddressBookInfo, string>(o => o.AddresserID)],
                           AddresserName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.AddressBook.AddressBookInfo, string>(o => o.AddresserName)],
                           AddresserNickName = dr["AddresserNickName"],
                           Remark = dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.AddressBook.AddressBookInfo, string>(o => o.Remark)]
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
    [WebMethod(Description = "获取我的通知列表", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetMyValidationMessageList()
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IcommunityInfo.GetPersonValidationMessageInfos(this.UserID);

        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityApplicationInfo, string>(o => o.ID)],
                           MemberID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityApplicationInfo, string>(o => o.MemberID)],
                           MemberName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityApplicationInfo, string>(o => o.MemberName)],
                           UserUrl = dr["IS_USER"].ToString() == ((int)MiicYesNoSetting.Yes).ToString() ? CommonService.GetManageFullUrl(dr["USER_URL"].ToString()) : dr["USER_URL"].ToString(),
                           UserName = dr["USER_NAME"],
                           CommunityID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityApplicationInfo, string>(o => o.CommunityID)],
                           UserType = dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.UserType)],
                           ApplicationTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityApplicationInfo, DateTime?>(o => o.ApplicationTime)],
                           ResponseTime = Convert.IsDBNull(dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityApplicationInfo, DateTime?>(o => o.ResponseTime)]) == true ? null : (DateTime?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityApplicationInfo, DateTime?>(o => o.ResponseTime)],
                           ResponseStatus = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityApplicationInfo, string>(o => o.ResponseStatus)],
                           Remark = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityApplicationInfo, string>(o => o.Remark)].ToString()
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
    [WebMethod(Description = "获取我的通知列表数", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public int GetMyValidationMessageCount()
    {
        return IcommunityInfo.GetPersonValidationMessageCount(this.UserID);
    }
    [WebMethod(Description = "是否可以创建行业圈子", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool CanCreateCommunity()
    {
        bool result = false;
        if (!string.IsNullOrEmpty(this.UserID))
        {
            result = IcommunityInfo.CanCreateCommunity(this.UserID);
        }
        return result;
    }
    [WebMethod(Description = "是否为管理员", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool IsAdmin(string userID, string communityID)
    {
        return IcommunityInfo.IsAdmin(userID, communityID);
    }
    [WebMethod(Description = "是否为创建者", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool IsCreater(string userID, string communityID)
    {
        return IcommunityInfo.IsCreater(userID, communityID);
    }

    [WebMethod(Description = "获取行业圈子统计信息", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(StatisticsSearchView))]
    public string GetCommunityStatistics(StatisticsSearchView searchView)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IcommunityInfo.GetCommunityToPersonStatistics(searchView);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       group dr by new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityStatistics, string>(o => o.ID)].ToString(),
                           CommunityName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityStatistics, string>(o => o.CommunityName)].ToString(),
                       } into k
                       select new
                       {
                           ID = k.Key.ID,
                           CommunityName = k.Key.CommunityName,
                           MemberList = (from item in k.AsEnumerable()
                                         select new
                                         {
                                             MemberID = item[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityStatistics, string>(o => o.MemberID)],
                                             MemberName = item[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityStatistics, string>(o => o.MemberName)],
                                             TopicCount = (int)item[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityStatistics, int?>(o => o.TopicCount)],
                                             PublishCount = (int)item[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityStatistics, int?>(o => o.PublishCount)]
                                         })
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }

    [WebMethod(Description = "获取行业圈子-标签统计信息", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(StatisticsSearchView))]
    public string GetCommunityToLabelStatistics(StatisticsSearchView searchView)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IcommunityInfo.GetCommunityToLabelStatistics(searchView);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       group dr by new
                       {
                           MemberID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityStatistics, string>(o => o.MemberID)],
                           MemberName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityStatistics, string>(o => o.MemberName)]
                       } into g
                       select new
                       {
                           MemberID = g.Key.MemberID,
                           MemberName = g.Key.MemberName,
                           LabelList = (from item in g.AsEnumerable()
                                        select new
                                        {
                                            LabelID = item["LABEL_ID"],
                                            LabelName = item["LABEL_NAME"],
                                            PublishCount = (int)item[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityStatistics, int?>(o => o.PublishCount)]
                                        })
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }

    [WebMethod(Description = "获取所有行业圈子包含标签信息", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(StatisticsSearchView))]
    public string GetAllCommunityWithLabelList()
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IcommunityInfo.GetAllCommunityWithLabelList();
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       group dr by new
                       {
                           CommunityID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.CommunityID)],
                           CommunityName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.Name)]
                       } into g
                       select new
                       {
                           CommunityID = g.Key.CommunityID,
                           CommunityName = g.Key.CommunityName,
                           LabelList = (from item in g.AsEnumerable()
                                        where Convert.IsDBNull(item[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.ID)]) == false
                                        select new
                                        {
                                            LabelID = item[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.ID)],
                                            LabelName = item[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.LabelName)]
                                        })
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
}
