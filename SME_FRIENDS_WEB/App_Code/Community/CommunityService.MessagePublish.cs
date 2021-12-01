using Miic.Base;
using Miic.Common;
using Miic.DB.SqlObject;
using Miic.Friends.Common;
using Miic.Friends.Community;
using Miic.Friends.Notice;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;

/// <summary>
/// Summary description for CommunityService
/// </summary>
public partial class CommunityService
{
    private static readonly IMessageInfo ImessageInfo = new Miic.Friends.Community.MessageInfoDao();
    [WebMethod(MessageName="TopicSubmit", BufferResponse = true, Description = "提交讨论信息")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(TopicInfo))]
    [GenerateScriptType(typeof(NoticeUserView))]
    public bool Submit(TopicInfo topicInfo, NoticeUserView noticeUserView)
    {
        bool result = false;
        TopicInfo tempInfo = ((ICommon<TopicInfo>)ImessageInfo).GetInformation(topicInfo.ID);
        if (tempInfo == null)
        {
            topicInfo.CreateTime = DateTime.Now;
            topicInfo.CreaterName =this.UserName;
            topicInfo.CreaterID = this.UserID;
            result = ImessageInfo.Insert(topicInfo, noticeUserView);
        }
        else
        {
            result = ImessageInfo.Update(topicInfo, noticeUserView);
        }
        return result;
    }

    [WebMethod(Description = "搜索我的最新动态讨论组", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(TopicSearchView))]
    [GenerateScriptType(typeof(MiicPage))]
    public string TopicSearch(TopicSearchView searchView, MiicPage page)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = ImessageInfo.Search(searchView, page);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       group dr by new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<TopicInfo, string>(o => o.ID)],
                           Content = dr[Config.Attribute.GetSqlColumnNameByPropertyName<TopicInfo, string>(o => o.TopicContent)],
                           MessageCount = dr[Config.Attribute.GetSqlColumnNameByPropertyName<TopicInfo, int?>(o => o.MessageCount)],
                           CreateTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<TopicInfo, DateTime?>(o => o.CreateTime)],
                           CreaterID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<TopicInfo, string>(o => o.CreaterID)],
                           CreaterName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<TopicInfo, string>(o => o.CreaterName)]
                       } into g
                       select new
                       {
                           ID = g.Key.ID,
                           Content = g.Key.Content,
                           MessageCount = g.Key.MessageCount,
                           CreateTime = g.Key.CreateTime,
                           CreaterID = g.Key.CreaterID,
                           CreaterName = g.Key.CreaterName,
                           MessageInfoList = g.Count(o => Convert.IsDBNull(o["MessageID"]) == false) == 0 ? null : (from item in g.AsEnumerable()
                                                                                                                    select new
                                                                                                                    {
                                                                                                                        Content = item[Config.Attribute.GetSqlColumnNameByPropertyName<MessageShowInfo, string>(o => o.Content)],
                                                                                                                        FromCommenterID = item[Config.Attribute.GetSqlColumnNameByPropertyName<MessageShowInfo, string>(o => o.FromCommenterID)],
                                                                                                                        FromCommenterName = item[Config.Attribute.GetSqlColumnNameByPropertyName<MessageShowInfo, string>(o => o.FromCommenterName)],
                                                                                                                        FromCommenterUrl = CommonService.GetManageFullUrl(item[Config.Attribute.GetSqlColumnNameByPropertyName<MessageShowInfo, string>(o => o.FromCommenterUrl)].ToString()),
                                                                                                                        FromCommenterRealName = item["FROM_COMMENTER_REAL_NAME"].ToString(),
                                                                                                                        FromCommenterIsFriend = item["FROM_COMMENTER_IS_FRIEND"].ToString(),
                                                                                                                        ToCommenterID = item[Config.Attribute.GetSqlColumnNameByPropertyName<MessageShowInfo, string>(o => o.ToCommenterID)],
                                                                                                                        ToCommenterName = item[Config.Attribute.GetSqlColumnNameByPropertyName<MessageShowInfo, string>(o => o.ToCommenterName)],
                                                                                                                        ToCommenterUrl = Convert.IsDBNull(item[Config.Attribute.GetSqlColumnNameByPropertyName<MessageShowInfo, string>(o => o.ToCommenterUrl)]) == true ? string.Empty : CommonService.GetManageFullUrl(item[Config.Attribute.GetSqlColumnNameByPropertyName<MessageShowInfo, string>(o => o.ToCommenterUrl)].ToString()),
                                                                                                                        CommentTime = item[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Group.MessageInfo, DateTime?>(o => o.CommentTime)],
                                                                                                                        ToCommenterRealName = item["TO_COMMENTER_REAL_NAME"].ToString(),
                                                                                                                        ToCommenterIsFriend = item["TO_COMMENTER_IS_FRIEND"].ToString()
                                                                                                                    }).Take(10)

                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }

    [WebMethod(Description = "获取搜索我的最新动态讨论组数", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(TopicSearchView))]
    public int GetTopicSearchCount(TopicSearchView searchView)
    {
        return ImessageInfo.GetSearchCount(searchView);
    }

    [WebMethod(Description = "根据讨论主题ID获取所有讨论交流信息列表", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MiicPage))]
    public string GetMessageInfoList(string topicID, MiicPage page)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = ImessageInfo.GetMessageListByTopicID(topicID, page);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<MessageShowInfo, string>(o => o.ID)],
                           TopicID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<MessageShowInfo, string>(o => o.TopicID)],
                           Content = dr[Config.Attribute.GetSqlColumnNameByPropertyName<MessageShowInfo, string>(o => o.Content)],
                           FromCommenterID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<MessageShowInfo, string>(o => o.FromCommenterID)],
                           FromCommenterName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<MessageShowInfo, string>(o => o.FromCommenterName)],
                           FromCommenterUrl = CommonService.GetManageFullUrl(dr[Config.Attribute.GetSqlColumnNameByPropertyName<MessageShowInfo, string>(o => o.FromCommenterUrl)].ToString()),
                           FromCommenterRealName = dr["FROM_USER_REAL_NAME"].ToString(),
                           FromCommenterIsFriend = dr["FROM_USER_IS_FRIEND"].ToString(),
                           ToCommenterID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<MessageShowInfo, string>(o => o.ToCommenterID)],
                           ToCommenterName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<MessageShowInfo, string>(o => o.ToCommenterName)],
                           ToCommenterUrl = Convert.IsDBNull(dr[Config.Attribute.GetSqlColumnNameByPropertyName<MessageShowInfo, string>(o => o.ToCommenterUrl)]) == true ? string.Empty : CommonService.GetManageFullUrl(dr[Config.Attribute.GetSqlColumnNameByPropertyName<MessageShowInfo, string>(o => o.ToCommenterUrl)].ToString()),
                           CommentTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.Group.MessageInfo, DateTime?>(o => o.CommentTime)],
                           ToCommenterRealName = dr["TO_USER_REAL_NAME"].ToString(),
                           ToCommenterIsFriend = dr["TO_USER_IS_FRIEND"].ToString()
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }

    [WebMethod(Description = "根据讨论主题ID获取讨论信息数", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public int GetMessageInfoCount(string topicID)
    {
        return ImessageInfo.GetMessageCountByTopicID(topicID);
    }

    [WebMethod(BufferResponse = true, Description = "发表讨论消息")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(ToCommentView))]
    public BoolWithPrimaryKeyView SubmitMessage(ToCommentView commentView)
    {
        Miic.Friends.Community.MessageInfo messageInfo = new Miic.Friends.Community.MessageInfo()
        {
            ID = Guid.NewGuid().ToString(),
            Content = commentView.Content,
            TopicID = commentView.PublishID,
            CommentTime = DateTime.Now,
            FromCommenterID = this.UserID,
            FromCommenterName = this.UserName,
            ToCommenterID = commentView.ToCommenterID,
            ToCommenterName = commentView.ToCommenterName
        };
        bool temp = ImessageInfo.Insert(messageInfo);
        BoolWithPrimaryKeyView result = new BoolWithPrimaryKeyView()
        {
            PrimaryID = temp == true ? messageInfo.ID : string.Empty,
            result = temp
        };
        return result;
    }

    [WebMethod(BufferResponse = true, Description = "撤回讨论消息")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool RemoveMessage(string messageID)
    {
        return ((ICommon<Miic.Friends.Community.MessageInfo>)ImessageInfo).Delete(messageID);
    }

    [WebMethod(BufferResponse = true, Description = "获取讨论信息")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(TopicInfo))]
    public TopicInfo GetTopic(string topicID)
    {
        return ((ICommon<Miic.Friends.Community.TopicInfo>)ImessageInfo).GetInformation(topicID);
    }

    [WebMethod(BufferResponse = true, Description = "删除讨论")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool DeleteTopic(string topicID)
    {
        return ((ICommon<Miic.Friends.Community.TopicInfo>)ImessageInfo).Delete(topicID);
    }
}