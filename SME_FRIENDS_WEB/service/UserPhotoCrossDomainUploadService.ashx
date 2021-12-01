<%@ WebHandler Language="C#" Class="UserPhotoCrossDomainUploadService" %>

using System;
using System.Web;
using System.Reflection;
using System.IO;
using Miic.Base;
using System.Web.Configuration;
public class UserPhotoCrossDomainUploadService : IHttpHandler
{
    private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
    private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
    private static readonly int UserPhotoFileVolumn = int.Parse(WebConfigurationManager.AppSettings["UserPhotoFileVolumn"].ToString());
    private static readonly string ManageUrl = WebConfigurationManager.AppSettings["ManageUrl"].ToString();
    private static readonly string fileTempPath = HttpContext.Current.Server.MapPath("/file/temp/User/");
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "text/plain";
        HttpPostedFile file = context.Request.Files[0] as HttpPostedFile;
        string fileExt = Path.GetExtension(file.FileName).TrimStart('.').ToLower();
        //上传头像
        if (!(fileExt == "jpg" || fileExt == "gif" || fileExt == "png"))
        {
            context.Response.Write(Config.Serializer.Serialize(new { result = false, message = "请选择合法的上传文件！" }));
            context.Response.End();
            return;
        }
        if (UserPhotoFileVolumn < file.ContentLength)
        {
            context.Response.Write(Config.Serializer.Serialize(new { result = false, message = "文件超限了！请上传小于" + UserPhotoFileVolumn / 1048576 + "M的文件。" }));
            context.Response.End();
            return;
        }
        string fileName = fileTempPath + file.FileName;
        if (!Directory.Exists(fileTempPath))
        {
            Directory.CreateDirectory(fileTempPath);
        }
        file.SaveAs(fileName);
        string responseResult = CommonService.CrossDomainUpload(ManageUrl+"/service/UserPhotoUploadService.ashx", fileName);
        File.Delete(fileName);
        if (responseResult == string.Empty)
        {
            context.Response.Write(Config.Serializer.Serialize(new { result = false, message = "传输失败！" }));
        }
        else
        {
            dynamic result = Config.Serializer.DeserializeObject(responseResult);
            if (result["result"] == true)
            {
                context.Response.Write(Config.Serializer.Serialize(new
                {
                    result = true,
                    acc = new
                    {
                        FileName = fileName,
                        FilePath = ManageUrl + "/file/User/" + result["ID"] + "." + fileExt,
                        FileExt = fileExt,
                        TempPath = ManageUrl + "/file/temp/User/" + result["ID"] + "." + fileExt,
                    }
                }));
            }
            else
            {
                context.Response.Write(Config.Serializer.Serialize(new { result = result["result"], message = result["message"] }));
            }
            context.Response.End();
            return;
        }
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}