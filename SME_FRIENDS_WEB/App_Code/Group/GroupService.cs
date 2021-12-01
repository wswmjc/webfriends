using Miic.Base;
using Miic.Base.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Group;
using Miic.Friends.SimpleGroup;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;

/// <summary>
/// 讨论组服务
/// </summary>
[WebService(Namespace = "http://pyq.mictalk.cn/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[ScriptService]
public partial class GroupService : WebService
{
    private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
    private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
    private static readonly IGroupInfo IgroupInfo = new GroupInfoDao();
    public string UserID { get; private set; }
    public string UserName { get; private set; }
    public GroupService()
    {
        string message = string.Empty;
        Cookie cookie = new Cookie();
        this.UserID = cookie.GetCookie("SNS_ID", out message);
        this.UserName = HttpUtility.UrlDecode(cookie.GetCookie("SNS_UserName", out message));
    }

    [WebMethod(Description = "添加讨论组", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(GroupInfo))]
    [GenerateScriptType(typeof(GroupMember))]
    public bool Add(GroupInfo groupInfo, List<GroupMember> members)
    {
        bool result = false;
        groupInfo.CreaterID = this.UserID;
        groupInfo.CreaterName = this.UserName;
        groupInfo.CreateTime = DateTime.Now;
        groupInfo.Valid = ((int)MiicValidTypeSetting.Valid).ToString();
        if (members != null)
        {
            groupInfo.MemberCount = members.Count + 1;
        }
        else 
        {
            groupInfo.MemberCount = 1;
        }
        if (members != null)
        {
            foreach (GroupMember item in members) 
            {
                item.JoinTime = DateTime.Now;
            }
            result = IgroupInfo.Insert(groupInfo, members);
        }
        else 
        {
            result = IgroupInfo.Insert(groupInfo);
        }
        return result;
    }
    [WebMethod(Description = "移除讨论组", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool Remove(string groupID)
    {
        return ((ICommon<GroupInfo>)IgroupInfo).Delete(groupID);
    }


    [WebMethod(Description = "搜索我的讨论组", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MySimpleGroupSearchView))]
    [GenerateScriptType(typeof(MiicPage))]
    public string Search(MySimpleGroupSearchView searchView, MiicPage page)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IgroupInfo.Search(searchView, page);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.ID)],
                           Name = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.Name)],
                           LogoUrl = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.LogoUrl)],
                           MemberCount = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, int?>(o => o.MemberCount)],
                           CreaterID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.CreaterID)],
                           CreaterName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.CreaterName)],
                           CreateTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, DateTime?>(o => o.CreateTime)],
                           GroupMemberID = dr["GroupMemberID"],
                           Remark = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.Remark)].ToString()
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
    [WebMethod(Description = "搜索我的讨论组数", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MySimpleGroupSearchView))]
    public int GetSearchCount(MySimpleGroupSearchView searchView)
    {
        return IgroupInfo.GetSearchCount(searchView);
    }
    [WebMethod(Description = "展示我的讨论组列表（右侧展示）", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string ShowMyGroupInfoList(int top)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IgroupInfo.GetGroupInfoList(this.UserID, top);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.ID)],
                           Name = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.Name)],
                           LogoUrl = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.LogoUrl)],
                           Remark = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.Remark)].ToString()
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
    [WebMethod(Description = "搜索我的最新动态讨论组", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MySimpleGroupSearchView))]
    [GenerateScriptType(typeof(MiicPage))]
    public string TrendsSearch(MySimpleGroupSearchView searchView, MiicPage page)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IgroupInfo.TrendsSearch(searchView, page);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       group dr by new
                       {
                           ID=dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo,string>(o=>o.ID)],
                           LogoUrl = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.LogoUrl)],
                           Name = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.Name)],
                           MemberCount = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, int?>(o => o.MemberCount)],
                           Manager = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.CreaterID)]
                       } into g
                       select new
                       {
                           ID=g.Key.ID,
                           LogoUrl = g.Key.LogoUrl,
                           Name = g.Key.Name,
                           MemberCount = g.Key.MemberCount,
                           Manager = g.Key.Manager,
                           TopicInfo = (from item in g.AsEnumerable()
                                       select new
                                       {
                                           Content = item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.TopicContent)],
                                           CreaterID = item["TopicCreaterID"],
                                           CreaterName = item["TopicCreaterName"],
                                           RealName = item["REAL_NAME"],
                                           IsFriend = item["IS_FRIEND"],
                                           CreaterUrl = CommonService.GetManageFullUrl(item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo, string>(o => o.CreaterUrl)].ToString()),
                                           CreateTime = item["TopicCreateTime"],
                                           MessageCount=item[Config.Attribute.GetSqlColumnNameByPropertyName<TopicShowInfo,int?>(o=>o.MessageCount)]
                                       }).Take(10)

                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
    [WebMethod(Description = "搜索我的最新动态讨论组数", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MySimpleGroupSearchView))]
    public int GetTrendsSearchCount(MySimpleGroupSearchView searchView)
    {
        return IgroupInfo.GetTrendsSearchCount(searchView);
    }
    [WebMethod(Description = "是否为创建者", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool IsCreater(string userID, string groupID)
    {
        return IgroupInfo.IsCreater(userID, groupID);
    }

    [WebMethod(BufferResponse = true, Description = "获取详细信息")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetGroupInfoByTopicID(string topicID)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IgroupInfo.GetDetailGroupByTopicID(topicID);
        var temp = from dr in dt.AsEnumerable()
                   select new
                    {
                        ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.ID)],
                        LogoUrl = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.LogoUrl)],
                        Name = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.Name)],
                        MemberCount = dr[Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, int?>(o => o.MemberCount)]
                    };

        result = Config.Serializer.Serialize(temp);
        return result;
    }
}
