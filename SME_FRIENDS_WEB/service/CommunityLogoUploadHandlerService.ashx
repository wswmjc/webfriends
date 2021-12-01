<%@ WebHandler Language="C#" Class="CommunityLogoUploadHandlerService" %>

using System;
using System.Web;
using System.Reflection;
using System.Web.Configuration;
using System.IO;
using Miic.Base;
using Miic.Log;
using System.Drawing;
public class CommunityLogoUploadHandlerService : IHttpHandler
{
    private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
    private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
    private static readonly int CommunityLogoVolumn = int.Parse(WebConfigurationManager.AppSettings["FileVolumn"].ToString());
    private static readonly string CommunityLogoPath = WebConfigurationManager.AppSettings["CommunityLogoPath"].ToString();
    private static readonly string CommunityLogoTempPath = CommunityLogoPath.Replace("/file", "/file/temp");
    /// <summary>
    /// 行业圈子Logo上传服务
    /// </summary>
    /// <param name="context">上下文</param>
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "text/plain";
        string filePath = HttpContext.Current.Server.MapPath(CommunityLogoTempPath);
        foreach (string item in context.Request.Files.AllKeys)
        {
            HttpPostedFile file = context.Request.Files[item];
            string fileExt = Path.GetExtension(file.FileName).TrimStart('.').ToLower();
            //文件类型校验
            if (!(fileExt == "jpg" || fileExt == "png"))//|| fileExt == "gif"
            {
                context.Response.Write(Config.Serializer.Serialize(new { result = false, message = "请选择合法的上传文件！" }));
                context.Response.End();
                return;
            }
            //尺寸大小校验
            Image temp = Image.FromStream(file.InputStream, true, true);
            int tempw = temp.Width;
            int temph = temp.Height;
            if ((tempw < 95 || tempw > 100) || (temph < 95 || temph > 100) || (tempw / (float)temph < 0.9 || tempw / (float)temph > 1.1))
            {
                context.Response.Write(Config.Serializer.Serialize(new { result = false, message = "尺寸要求长为95~100,宽为95~100,长/宽为0.9~1.1,建议上传为95*95的图片！" }));
                context.Response.End();
                return;
            }
            //文件大小
            if (CommunityLogoVolumn < file.ContentLength)
            {
                context.Response.Write(Config.Serializer.Serialize(new { result = false, message = "文件超限了！请上传小于" + CommunityLogoVolumn / 1048576 + "M的文件。" }));
                context.Response.End();
                return;
            }
            string fileName = Path.GetFileNameWithoutExtension(file.FileName);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            string newName = Guid.NewGuid().ToString();
            string newFileName = newName + "." + fileExt;
            try
            {
                file.SaveAs(filePath + newFileName);
                context.Response.Write(Config.Serializer.Serialize(new
                {
                    result = true,
                    acc = new
                    {
                        FileName = file.FileName,
                        FilePath = CommunityLogoPath + newFileName,
                        FileExt = fileExt,
                        TempPath = CommunityLogoTempPath + newFileName
                    }
                }));
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
        }
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

}