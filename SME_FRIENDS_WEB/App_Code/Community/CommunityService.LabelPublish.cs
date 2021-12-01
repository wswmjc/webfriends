using Miic.Base;
using Miic.Base.Setting;
using Miic.DB.SqlObject; 
using Miic.Friends.Community;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Script.Services;
using System.Web.Services;


/// <summary>
/// Summary description for CommunityService
/// </summary>
public partial class CommunityService
{
    private static readonly ILabelInfo IlabelInfo = new LabelInfoDao();
    [WebMethod(MessageName = "LabelSearch", Description = "搜索行业圈子标签主题", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MiicPage))]
    [GenerateScriptType(typeof(NoPersonKeywordView))]
    public string LabelSearch(NoPersonKeywordView keywordView, MiicPage page)
    {
        string result =CommonService.InitialJsonList;
        DataTable dt = IlabelInfo.Search(keywordView, page);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       group dr by new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.ID)],
                           Name = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.LabelName)],
                           CommunityID = keywordView.CommunityID,
                           CreaterID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.CreaterID)],
                           CreaterName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.CreaterName)],
                           CreateTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, DateTime?>(o => o.CreateTime)]
                       } into g
                       select new
                       {
                           ID = g.Key.ID,
                           Name = g.Key.Name,
                           CommunityID = g.Key.CommunityID,
                           CreaterID = g.Key.CreaterID,
                           CreaterName = g.Key.CreaterName,
                           CreateTime = g.Key.CreateTime,
                           PublishCount = (from item in g.AsEnumerable()
                                           where Convert.IsDBNull(item[Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(u => u.PublishID)]) == false
                                           select new
                                           {
                                               PublishID = item[Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.PublishID)]
                                           }).Distinct().Count(),
                           UserList = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(u => u.PublishID)]) == false) == 0 ? null : (from item in g.AsEnumerable()
                                                                                                                                                                                            select new
                                                                                                                                                                                            {
                                                                                                                                                                                                UserID = item[Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.UserID)],
                                                                                                                                                                                                UserName = item[Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.UserName)],
                                                                                                                                                                                                UserType = item[Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.UserType)],
                                                                                                                                                                                                UserUrl = CommonService.GetManageFullUrl(item[Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.UserUrl)].ToString())
                                                                                                                                                                                            }).Distinct().Take(14)
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
    [WebMethod(MessageName = "GetLabelSearchCount", Description = "根据搜索关键字获取行业圈子标签主题数", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(NoPersonKeywordView))]
    public int GetLabelSearchCount(NoPersonKeywordView keywordView)
    {
        return IlabelInfo.GetSearchCount(keywordView);
    }
    [WebMethod(MessageName="AddLabel",Description = "添加标签", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(LabelInfo))]
    public bool Add(LabelInfo labelInfo) 
    {
        labelInfo.CreaterID = this.UserID;
        labelInfo.CreaterName = this.UserName;
       return   IlabelInfo.Insert(labelInfo);
    }
    [WebMethod(MessageName = "RemoveLabel", Description = "删除标签", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool RemoveLabel(string id) 
    {
        return IlabelInfo.Delete(id);
    }

    [WebMethod(MessageName = "GetCommunityLabelList", Description = "搜索行业圈子标签列表", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetCommunityLabelList(string communityID)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IlabelInfo.GetLabelInfosByCommunityID(communityID);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.ID)],
                           Name = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.LabelName)],
                           CommunityID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.CommunityID)],
                           CreaterID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.CreaterID)],
                           CreaterName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.CreaterName)],
                           CreateTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, DateTime?>(o => o.CreateTime)],
                           EndTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, DateTime?>(o => o.EndTime)],
                           Valid = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.Valid)]
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
    [WebMethod(Description = "根据标签获取博文信息", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MiicPage))]
    public string GetPublishInfosByLabel(string labelID,MiicPage page) 
    {
        string result = CommonService.InitialJsonObject;
        DataTable dt = new DataTable();
        if (dt.Rows.Count > 0) 
        {
            var temp = from dr in dt.AsEnumerable()
                       group dr by new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID)],
                           Title = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Title)],
                           Content = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Content)],
                           CreaterID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.CreaterID)],
                           CreaterName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.CreaterName)],
                           CreateTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.CreateTime)],
                           HasAcc = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.HasAcc)],
                           PraiseNum = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.PraiseNum)],
                           TreadNum = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.TreadNum)],
                           TransmitNum = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.TransmitNum)],
                           ReportNum = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.ReportNum)],
                           CommentNum = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.CommentNum)]
                       } into g
                       select new
                       {
                           ID = g.Key.ID,
                           Title = g.Key.Title,
                           Content = g.Key.Content,
                           CreaterID = g.Key.CreaterID,
                           CreaterName = g.Key.CreaterName,
                           CreateTime = g.Key.CreateTime,
                           HasAcc = g.Key.HasAcc,
                           PraiseNum = g.Key.PraiseNum,
                           TreadNum = g.Key.TreadNum,
                           TransmitNum = g.Key.TransmitNum,
                           ReportNum = g.Key.ReportNum,
                           CommentNum = g.Key.CommentNum,
                           FileList = g.Key.HasAcc.ToString() == ((int)MiicYesNoSetting.No).ToString() ? 
                                       null :
                                       (from item in g.AsEnumerable() 
                                        select new 
                                        { 
                                           Name=item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo,string>(o=>o.FileName)],
                                           Path=item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo,string>(o=>o.FilePath)],
                                           FileType=item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo,string>(o=>o.FileType)]
                                        })
                       };
        }
        return result;
    }
    [WebMethod(Description = "根据标签获取博文数", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public int GetPublishInfosCountByLabel(string labelID) 
    {
        return IcommunityInfo.GetPublishInfosCountByLabelID(labelID);
    }

    [WebMethod(Description = "搜索行业圈子被引用的标签主题", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetLabelListWithIDs(List<string> ids)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = IlabelInfo.GetLabelListWithIDs(ids);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       group dr by new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.ID)],
                           Name = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.LabelName)],
                           CommunityID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.CommunityID)],
                           CreaterID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.CreaterID)],
                           CreaterName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.CreaterName)],
                           CreateTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, DateTime?>(o => o.CreateTime)]
                       } into g
                       select new
                       {
                           ID = g.Key.ID,
                           Name = g.Key.Name,
                           CommunityID = g.Key.CommunityID,
                           CreaterID = g.Key.CreaterID,
                           CreaterName = g.Key.CreaterName,
                           CreateTime = g.Key.CreateTime,
                           PublishCount = (from item in g.AsEnumerable()
                                           where Convert.IsDBNull(item[Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(u => u.PublishID)]) == false
                                           select new
                                           {
                                               PublishID = item[Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.PublishID)]
                                           }).Distinct().Count(),
                           UserList = g.Count(o => Convert.IsDBNull(o[Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(u => u.PublishID)]) == false) == 0 ? null : (from item in g.AsEnumerable()
                                                                                                                                                                                            select new
                                                                                                                                                                                            {
                                                                                                                                                                                                UserID = item[Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.UserID)],
                                                                                                                                                                                                UserName = item[Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.UserName)],
                                                                                                                                                                                                UserType = item[Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.UserType)],
                                                                                                                                                                                                UserUrl = CommonService.GetManageFullUrl(item[Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.UserUrl)].ToString())
                                                                                                                                                                                            }).Distinct().Take(14)
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
}