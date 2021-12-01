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

/// <summary>
///消息服务
/// </summary>
[WebService(Namespace = "http://pyq.mictalk.cn/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
 [ScriptService]
public class MessageService :WebService {
    private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
    private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
    private static readonly INoticeInfo<MessageInfo> ImessageInfo = new MessageInfoDao();
    public MessageService () {

    }
    [WebMethod(Description = "查询我的消息", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MyNoticeView))]
    [GenerateScriptType(typeof(MiicPage))]
    public string Search(MyNoticeView myNoticeView, MiicPage page)
    {
        string result = CommonService.InitialJsonList;
        DataTable dt = ImessageInfo.GetMyNoticeInfoList(myNoticeView, page);
        if (dt.Rows.Count > 0)
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<MessageInfo, string>(o => o.ID)],
                           PublishID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<MessageInfo, string>(o => o.PublishID)],
                           Source = dr[Config.Attribute.GetSqlColumnNameByPropertyName<MessageInfo, string>(o => o.Source)],
                           UserID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<MessageInfo, string>(o => o.PublisherID)],
                           UserName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<MessageInfo, string>(o => o.PublisherName)],
                           PublishTime = dr[Config.Attribute.GetSqlColumnNameByPropertyName<MessageInfo, DateTime?>(o => o.PublishTime)]
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
    [WebMethod(Description = "查询我的消息数", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MyNoticeView))]
    public int GetSearchCount(MyNoticeView myNoticeView)
    {
        return ImessageInfo.GetMyNoticeInfoCount(myNoticeView);
    }
    [WebMethod(Description = "阅读信息", BufferResponse = true)]
    public bool ReadNotice(string id)
    {
        return ImessageInfo.Update(new MessageInfo()
        {
            ID = id,
            ReadTime = DateTime.Now,
            ReadStatus = ((int)MiicReadStatusSetting.Read).ToString()
        });
    }
    [WebMethod(Description = "忽略信息", BufferResponse = true)]
    public bool IgnoreNotice(string id)
    {
        return ImessageInfo.Update(new MessageInfo()
        {
            ID = id,
            ReadTime = DateTime.Now,
            ReadStatus = ((int)MiicReadStatusSetting.Ignore).ToString()
        });
    }
    
}
