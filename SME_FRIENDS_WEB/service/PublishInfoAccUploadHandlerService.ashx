<%@ WebHandler Language="C#" Class="PublishInfoAccUploadHandlerService" %>

using System;
using System.Web;
using System.Web.Configuration;
using System.Reflection;
using System.IO;
using System.Drawing;
using Miic.Base;
using Miic.Draw;
using Miic.BaseStruct.Draw;
using Miic.Friends.Common.Setting;
public class PublishInfoAccUploadHandlerService : IHttpHandler
{
    private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
    private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
    private static readonly int InformationFileVolumn = int.Parse(WebConfigurationManager.AppSettings["FileVolumn"].ToString());
    public void ProcessRequest(HttpContext context)
    {
        string type = string.Empty; ;
        if (context.Request["Type"] != null)
        {
            type = context.Request["Type"].ToString();
            ///Type                               
            ///->Photo   :信息附件图片（图片）    
            ///->File    :信息附件（文件）   
            ///->LongInfoImage     :长篇内容插入图片（图片）
            ///->others            :待添加

            if (!(type == "Photo"
                || type == "File"
                || type == "LongInfoImage" 
                || type == "others"))
            {
                context.Response.Write(Config.Serializer.Serialize(new { result = false, message = "您选择类型有误！" }));
                context.Response.End();
                return;
            }
        }
        else
        {
            context.Response.Write(Config.Serializer.Serialize(new { result = false, message = "请选择类型" }));
            context.Response.End();
            return;
        }

        if (context.Request.Files.Count == 0)
        {
            context.Response.Write(Config.Serializer.Serialize(new { result = false, message = "未上传任何附件" }));
            context.Response.End();
            return;
        }

        HttpPostedFile uploadFile = context.Request.Files[0] as HttpPostedFile;
        string filePath = HttpContext.Current.Server.MapPath("/file/temp/PublishInfoAcc/" + type + "/");
        if (type == "LongInfoImage")
        {
            filePath = HttpContext.Current.Server.MapPath("/file/PublishInfoAcc/" + type + "/");
        }
        string fileExt = Path.GetExtension(uploadFile.FileName).TrimStart('.').ToLower();
        AccFileTypeSetting fileType = AccFileTypeSetting.Accessory;
        if (type == "Photo" || type == "LongInfoImage")
        {
            //图片类别判断
            if (!(fileExt.ToLower() == "jpg" || fileExt.ToLower() == "jpeg" || fileExt.ToLower() == "gif" || fileExt.ToLower() == "png"))
            {
                context.Response.Write(Config.Serializer.Serialize(new { result = false, message = "请选择合法的上传文件！" }));
                context.Response.End();
                return;
            }
            else
            {
                fileType = AccFileTypeSetting.Photo;
            }
        }
        else if (type == "File")
        {
            if (!(fileExt == "doc" || fileExt == "docx"
             || fileExt == "xls" || fileExt == "xlsx"
             || fileExt == "ppt" || fileExt == "pptx"
             || fileExt == "pdf"
             || fileExt == "txt"
             || fileExt == "rar" || fileExt == "zip"))
            {
                context.Response.Write(Config.Serializer.Serialize(new { result = false, message = "请选择合法的上传文件！" }));
                context.Response.End();
                return;
            }
            else
            {
                if ((fileExt == "doc" || fileExt == "docx")
                    ||(fileExt == "xls" || fileExt == "xlsx")
                    || (fileExt == "ppt" || fileExt == "pptx")
                    || fileExt == "pdf"
                    || fileExt == "txt"
                    || (fileExt == "rar" || fileExt == "zip")
                    )
                {
                    fileType = CommonService.ConvertAccFileType(fileExt);
                }
            }
        }
        else
        {//other
            if (!(fileExt == "jpg" || fileExt == "gif" || fileExt == "png"
           || fileExt == "doc" || fileExt == "docx"
           || fileExt == "xls" || fileExt == "xlsx"
           || fileExt == "ppt" || fileExt == "pptx"
           || fileExt == "pdf"
           || fileExt == "txt"
           || fileExt == "rar" || fileExt == "zip"))
            {
                context.Response.Write(Config.Serializer.Serialize(new { result = false, message = "请选择合法的上传文件！" }));
                context.Response.End();
                return;
            }
            else
            {
                if ((fileExt == "doc" || fileExt == "docx")
                    || (fileExt == "xls" || fileExt == "xlsx")
                    || (fileExt == "ppt" || fileExt == "pptx")
                    || fileExt == "pdf"
                    || fileExt == "txt"
                    || (fileExt == "rar" || fileExt == "zip")
                    )
                {
                    fileType = CommonService.ConvertAccFileType(fileExt);
                }

            }
        }

        if (InformationFileVolumn < uploadFile.ContentLength)
        {
            context.Response.Write(Config.Serializer.Serialize(new { result = false, message = "文件超限了！请上传小于" + InformationFileVolumn / 1048576 + "M的文件。" }));
            context.Response.End();
            return;
        }
        string fileName = Path.GetFileNameWithoutExtension(uploadFile.FileName);
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        string newName = Guid.NewGuid().ToString();
        string newFileName = newName + "." + fileExt;
        uploadFile.SaveAs(filePath + newFileName);
        context.Response.Write(Config.Serializer.Serialize(new
        {
            result = true,
            acc = new
            {
                FileName = fileName,
                FilePath = "/file/PublishInfoAcc/" + type + "/" + newFileName,
                FileExt = fileExt,
                FileType = fileType,
                TempPath = "/file/temp/PublishInfoAcc/" + type + "/" + newFileName
            }
        }));
        context.Response.End();
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

}