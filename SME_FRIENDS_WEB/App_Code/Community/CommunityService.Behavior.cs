using Miic.Base;
using Miic.Base.ConfigSection;
using Miic.Base.Setting;
using Miic.Common;
using Miic.DB.SqlObject;
using Miic.Friends.Common;
using Miic.Friends.Community;
using Miic.Friends.Community.Behavior;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Services;
using System.Web.Services;

/// <summary>
/// Summary description for CommunityService
/// </summary>
public partial class CommunityService
{
    private static readonly ICommunityBehavior<CollectInfo> IcollectInfo = new CollectInfoDao();
    private static readonly ICommunityBehavior<PraiseInfo> IpraiseInfo = new PraiseInfoDao();
    private static readonly ICommunityBehavior<BrowseInfo> IbrowseInfo = new BrowseInfoDao();
    private static readonly ICommunityBehavior<TreadInfo> ItreadInfo = new TreadInfoDao();
    private static readonly ICommunityBehavior<ReportInfo> IreportInfo = new ReportInfoDao();
    private static readonly ICommunityBehavior<CommentInfo> IcommentInfo = new CommentInfoDao();
    private static readonly AnonymousUserConfigSection anonymousUserConfigSection = (AnonymousUserConfigSection)WebConfigurationManager.GetSection("AnonymousUserConfigSection");

    [WebMethod(BufferResponse = true, Description = "浏览")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(BoolWithPrimaryKeyView))]
    public BoolWithPrimaryKeyView AddBrowse(string publishID)
    {
        string BrowserID;
        string BrowserName;
        //将当前用户的浏览记录计入数据库
        if (!string.IsNullOrEmpty(UserID))
        {
            BrowserID = UserID;
            BrowserName = UserName;
        }
        else
        {
            //如果当前没有人登录，则按照匿名用户浏览
            BrowserID = anonymousUserConfigSection.UserID;
            BrowserName = anonymousUserConfigSection.UserName;
        }
        //获得浏览存储的信息
        //线上活动ID、浏览人的ID与名称、浏览时间、是否失效
        BrowseInfo browseInfo = new BrowseInfo()
        {
            ID = Guid.NewGuid().ToString(),
            PublishID = publishID,
            BrowserID = BrowserID,
            BrowserName = BrowserName,
            BrowseTime = DateTime.Now,
            BrowserIP = HttpContext.Current.Request.UserHostAddress,
            IsHinted = ((int)MiicYesNoSetting.Yes).ToString()
        };
        bool browseResult = IbrowseInfo.Insert(browseInfo);
        BoolWithPrimaryKeyView result = new BoolWithPrimaryKeyView()
        {
            PrimaryID = browseResult == true ? browseInfo.ID : string.Empty,
            result = browseResult
        };
        return result;
    }
    [WebMethod(BufferResponse = true, Description = "点赞或取消赞")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(BoolWithPrimaryKeyView))]
    public BoolWithPrimaryKeyView Praise(string publishID)
    {
        PraiseInfo tempPraiseInfo = ((PraiseInfoDao)IpraiseInfo).GetUserPraiseInfo(new MyCommunityBehaviorView()
        {
            PublishID = publishID
        });
        bool operResult = false;
        string operID = string.Empty;
        if (tempPraiseInfo == null)
        {//点赞操作
            PraiseInfo praiseInfo = new PraiseInfo()
            {
                ID = Guid.NewGuid().ToString(),
                PublishID = publishID,
                PraiserID = this.UserID,
                PraiserName = this.UserName,
                PraiseTime = DateTime.Now,
            };
            operID = praiseInfo.ID;
            operResult = IpraiseInfo.Insert(praiseInfo);
        }
        else
        {//取消赞操作
            operResult = IpraiseInfo.Delete(tempPraiseInfo.ID);
        }

        BoolWithPrimaryKeyView result = new BoolWithPrimaryKeyView()
        {
            PrimaryID = operID,//取消赞时为空，点赞时为新增记录ID
            result = operResult
        };
        return result;
    }

    [WebMethod(BufferResponse = true, Description = "点踩或者取消踩")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(BoolWithPrimaryKeyView))]
    public BoolWithPrimaryKeyView Tread(string publishID)
    {
        TreadInfo tempThreadInfo = ((TreadInfoDao)ItreadInfo).GetUserTreadInfo(new MyCommunityBehaviorView()
        {
            PublishID = publishID
        });
        bool operResult = false;
        string operID = string.Empty;
        if (tempThreadInfo == null)
        {
            TreadInfo treadInfo = new TreadInfo()
            {
                ID = Guid.NewGuid().ToString(),
                PublishID = publishID,
                TreaderID = this.UserID,
                TreaderName = this.UserName,
                TreadTime = DateTime.Now
            };
            operID = treadInfo.ID;
            operResult = ItreadInfo.Insert(treadInfo);
        }
        else
        {
            operResult = ItreadInfo.Delete(tempThreadInfo.ID);
        }

        BoolWithPrimaryKeyView result = new BoolWithPrimaryKeyView()
        {
            PrimaryID = operID,
            result = operResult
        };
        return result;
    }

    [WebMethod(BufferResponse = true, Description = "举报或者取消举报")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(BoolWithPrimaryKeyView))]
    public BoolWithPrimaryKeyView Report(string publishID)
    {
        ReportInfo tempReportInfo = ((ReportInfoDao)IreportInfo).GetUserReportInfo(new MyCommunityBehaviorView()
        {
            PublishID = publishID
        });
        bool operResult = false;
        string operID = string.Empty;
        if (tempReportInfo == null)
        {
            ReportInfo reportInfo = new ReportInfo()
            {
                ID = Guid.NewGuid().ToString(),
                PublishID = publishID,
                ReporterID = this.UserID,
                ReporterName =this.UserName,
                ReportTime = DateTime.Now
            };

            operID = reportInfo.ID;
            operResult = IreportInfo.Insert(reportInfo);
        }
        else
        {
            operResult = IreportInfo.Delete(tempReportInfo.ID);
        }

        BoolWithPrimaryKeyView result = new BoolWithPrimaryKeyView()
        {
            PrimaryID = operID,
            result = operResult
        };
        return result;
    }

    [WebMethod(BufferResponse = true, Description = "收藏或取消收藏")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(BoolWithPrimaryKeyView))]
    public BoolWithPrimaryKeyView Collect(string publishID)
    {
        CollectInfo tempCollectInfo = ((CollectInfoDao)IcollectInfo).GetUserCollectInfo(new MyCommunityBehaviorView()
        {
            PublishID = publishID
        });
        bool operResult = false;
        string operID = string.Empty;
        if (tempCollectInfo == null)
        {
            CollectInfo collectInfo = new CollectInfo()
            {
                ID = Guid.NewGuid().ToString(),
                CollectorID = this.UserID,
                CollectorName =this.UserName,
                CollectTime = DateTime.Now,
                PublishID = publishID,
                CollectValid = ((int)MiicValidTypeSetting.Valid).ToString()
            };
            operID = collectInfo.ID;
            operResult = IcollectInfo.Insert(collectInfo);
        }
        else
        {
            operResult = IcollectInfo.Delete(tempCollectInfo.ID);
        }

        BoolWithPrimaryKeyView result = new BoolWithPrimaryKeyView()
        {
            PrimaryID = operID,
            result = operResult
        };
        return result;
    }

    [WebMethod(BufferResponse = true, Description = "取消收藏")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool CancelCollect(string collectID)
    {
        return IcollectInfo.Delete(collectID);
    }

    [WebMethod(BufferResponse = true, Description = "获取我的收藏信息列表")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MyKeywordView))]
    [GenerateScriptType(typeof(MiicPage))]
    public string GetCollectInfos(MyKeywordView keywordView, MiicPage page)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = ((CollectInfoDao)IcollectInfo).GetCollectInfos(keywordView, page);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       group dr by
                       new
                       {
                           ID = dr["CommunityCollectInfo" + Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.ID)].ToString(),
                           PublishInfoID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID)].ToString(),
                           Title = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Title)].ToString(),
                           DetailContent = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Content)].ToString(),
                           Content = CommonService.DelImgStr(dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.PublishType)].ToString(), dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Content)].ToString(), false),
                           CreaterID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.CreaterID)].ToString(),
                           CreaterName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.CreaterName)].ToString(),
                           CreaterOrgName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.OrgName)].ToString(),
                           CreaterUserUrl = CommonService.GetManageFullUrl(dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.UserUrl)].ToString()),
                           CreaterUserType = dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.UserType)].ToString(),
                           CreateTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.CreateTime)],
                           EditStatus = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.EditStatus)].ToString(),
                           PublishTime = (DateTime?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.PublishTime)],
                           PublishType = dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.PublishType)].ToString(),
                           BrowseNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.BrowseNum)],
                           PraiseNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.PraiseNum)],
                           TreadNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.TreadNum)],
                           TransmitNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.TransmitNum)],
                           CollectNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.CollectNum)],
                           CommentNum = (int?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, int?>(o => o.CommentNum)]
                       } into g
                       select new
                       {
                           ID = g.Key.ID,
                           PublishInfoID = g.Key.PublishInfoID,
                           Title = g.Key.Title,
                           DetailContent = g.Key.DetailContent,
                           Content = g.Key.Content,
                           CreaterID = g.Key.CreaterID,
                           CreaterName = g.Key.CreaterName,
                           CreaterOrgName = g.Key.CreaterOrgName,
                           CreaterUserUrl = g.Key.CreaterUserUrl,
                           CreaterUserType = g.Key.CreaterUserType,
                           CreateTime = g.Key.CreateTime,
                           EditStatus = g.Key.EditStatus,
                           PublishTime = g.Key.PublishTime,
                           PublishType = g.Key.PublishType,
                           BrowseNum = g.Key.BrowseNum,
                           PraiseNum = g.Key.PraiseNum,
                           TreadNum = g.Key.TreadNum,
                           TransmitNum = g.Key.TransmitNum,
                           CollectNum = g.Key.CollectNum,
                           IsCollect = (from item in g.AsParallel()
                                        where Convert.IsDBNull(item[Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.CollectorID)]) == false
                                      && item[Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.CollectorID)].ToString() == UserID
                                        select new
                                        {
                                            CollectorID = item[Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.CollectorID)].ToString()
                                        }).Distinct().Count() == 0 ? false : true,
                           AccInfos = (from item in g
                                       where Convert.IsDBNull(item["CommunityAccessoryInfo" + Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID)]) == false
                                       select new
                                       {
                                           MicroAccessoryInfoID = item["CommunityAccessoryInfo" + Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID)].ToString(),
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

    [WebMethod(BufferResponse = true, Description = "获取我的收藏信息数")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MyKeywordView))]
    public int GetCollectInfoCount(MyKeywordView keywordView)
    {
        return ((CollectInfoDao)IcollectInfo).GetCollectCount(keywordView);
    }

    [WebMethod(MessageName = "PersonComment", BufferResponse = true, Description = "评论")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(ToCommentView), ScriptTypeId = "PersonToPerson")]
    public BoolWithPrimaryKeyView Comment(ToCommentView commentView)
    {
        CommentInfo comment = new CommentInfo()
        {
            ID = Guid.NewGuid().ToString(),
            Content = commentView.Content,
            PublishID = commentView.PublishID,
            CommentTime = DateTime.Now,
            FromCommenterID = this.UserID,
            FromCommenterName = this.UserName,
            ToCommenterID = commentView.ToCommenterID,
            ToCommenterName = commentView.ToCommenterName
        };
        bool temp = IcommentInfo.Insert(comment);
        BoolWithPrimaryKeyView result = new BoolWithPrimaryKeyView()
        {
            PrimaryID = temp == true ? comment.ID : string.Empty,
            result = temp
        };
        return result;
    }

    [WebMethod(MessageName = "BroadcastComment", BufferResponse = true, Description = "广播")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(BroadcastCommentView), ScriptTypeId = "Broadcast")]
    public BoolWithPrimaryKeyView Comment(BroadcastCommentView commentView)
    {
        CommentInfo comment = new CommentInfo()
        {
            ID = Guid.NewGuid().ToString(),
            Content = commentView.Content,
            PublishID = commentView.PublishID,
            CommentTime = DateTime.Now,
            FromCommenterID = this.UserID,
            FromCommenterName = this.UserName,
        };
        bool temp = IcommentInfo.Insert(comment);


        BoolWithPrimaryKeyView result = new BoolWithPrimaryKeyView()
        {
            PrimaryID = temp == true ? comment.ID : string.Empty,
            result = temp
        };
        return result;
    }

    [WebMethod(BufferResponse = true, Description = "撤回我的评论")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool RemoveMyComment(string commentID)
    {
        return IcommentInfo.Delete(commentID);
    }

    [WebMethod(BufferResponse = true, Description = "根据朋友圈信息ID获取评论")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MiicPage))]
    public string GetCommentInfos(string publishID, MiicPage page)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = ((CommentInfoDao)IcommentInfo).GetCommentList(publishID, page);
        if (dt != null && dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommentInfo, string>(o => o.ID)].ToString(),
                           PublishID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommentInfo, string>(o => o.PublishID)].ToString(),
                           Content = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommentInfo, string>(o => o.Content)].ToString(),
                           FromCommenterID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommentInfo, string>(o => o.FromCommenterID)].ToString(),
                           FromCommenterName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommentInfo, string>(o => o.FromCommenterName)].ToString(),
                           FromCommenterUrl = CommonService.GetManageFullUrl(dr["FROM_USER_URL"].ToString()),
                           FromCommenterType = dr["FROM_USER_TYPE"].ToString(),
                           FromCommenterRemark = dr["FROM_USER_REMARK"].ToString(),
                           FromCommenterRealName = dr["FROM_USER_REAL_NAME"].ToString(),
                           FromCommenterIsFriend = dr["FROM_USER_IS_FRIEND"].ToString(),
                           CommentTime = (DateTime?)dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommentInfo, DateTime?>(o => o.CommentTime)],
                           ToCommenterID = Convert.IsDBNull(dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommentInfo, string>(o => o.ToCommenterID)]) ? string.Empty : dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommentInfo, string>(o => o.ToCommenterID)].ToString(),
                           ToCommenterName = Convert.IsDBNull(dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommentInfo, string>(o => o.ToCommenterName)]) ? string.Empty : dr[Config.Attribute.GetSqlColumnNameByPropertyName<CommentInfo, string>(o => o.ToCommenterName)].ToString(),
                           ToCommenterUrl = CommonService.GetManageFullUrl(dr["TO_USER_URL"].ToString()),
                           ToCommenterType = dr["TO_USER_TYPE"].ToString(),
                           ToCommenterRemark = dr["TO_USER_REMARK"].ToString(),
                           ToCommenterRealName = dr["TO_USER_REAL_NAME"].ToString(),
                           ToCommenterIsFriend = dr["TO_USER_IS_FRIEND"].ToString()
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }

    [WebMethod(BufferResponse = true, Description = "根据朋友圈信息获取评论数")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public int GetCommentInfoCount(string publishID)
    {
        return ((CommentInfoDao)IcommentInfo).GetCommentCount(publishID);
    }


    [WebMethod(BufferResponse = true, Description = "设置已经发布的信息上下线")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(EditStatusView))]
    public bool SetEditStatus(EditStatusView editStatusView)
    {
        return IpublishInfo.SetEditStatus(editStatusView);
    }
}