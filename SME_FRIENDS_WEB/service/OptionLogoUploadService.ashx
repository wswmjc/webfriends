<%@ WebHandler Language="C#" Class="OptionLogoUploadService" %>

using System;
using System.Web;
using System.Web.Configuration;
using System.Reflection;
using System.IO;
using Miic.Base;

public class OptionLogoUploadService : IHttpHandler
{
    private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
    private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
    private static readonly int InformationFileVolumn = int.Parse(WebConfigurationManager.AppSettings["FileVolumn"].ToString());
    public void ProcessRequest (HttpContext context) {
        context.Response.ContentType = "text/plain";
        string filePath = string.Empty;
        foreach (string item in context.Request.Files.AllKeys)
        {
            HttpPostedFile file = context.Request.Files[item];
            string fileExt = Path.GetExtension(file.FileName).TrimStart('.').ToLower();

            if (fileExt == "jpg" || fileExt == "gif" || fileExt == "png")
            {//上传操作图标图片
                filePath = HttpContext.Current.Server.MapPath("/file/temp/Theme/OptionLogoUrl/");
            }
            else
            {
                context.Response.Write(Config.Serializer.Serialize(new { result = false, message = "请选择合法的上传文件！" }));
                context.Response.End();
                return;
            }

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            string newName = Guid.NewGuid().ToString();
            string newFileName = newName + "." + fileExt;
            string id = newName;
            try
            {
                file.SaveAs(filePath + newFileName);
                context.Response.Write(Config.Serializer.Serialize(new { result = true, ID = id }));
            }
            catch (Exception ex)
            {
                Config.IlogicLogService.Write(new Miic.Log.LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                });
            }
        }
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

}