using Miic.Friends.Common.Setting;
using Miic.Log;
using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
/// <summary>
/// Summary description for CommonService
/// </summary>
namespace Miic.Base
{
    public class CommonService
    {
        private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
        private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
        public static readonly string InitialJsonList = "[]";
        public static readonly string InitialJsonObject = "{}";
        public CommonService()
        {

        }
        /// <summary>
        /// 获取超出长度文章省略号表达
        /// </summary>
        /// <param name="publishInfoType">发布类型</param>
        /// <param name="title">标题</param>
        /// <param name="content">内容</param>
        /// <param name="abbrLength">提取长度</param>
        /// <returns></returns>
        public static string GetAbbrTitle(string publishInfoType, string title, string content, int abbrLength)
        {
            string result = string.Empty;
            if (publishInfoType != ((int)PublishInfoTypeSetting.Short).ToString())
            {
                result = title.Substring(0, title.Length > abbrLength ? abbrLength : title.Length) + (title.Length > abbrLength ? "..." : string.Empty);
            }
            else
            {
                string regexstr = @"<[^>]*>";    //去除所有的标签
                result = Regex.Replace(content, regexstr, string.Empty, RegexOptions.IgnoreCase);
                result = result.Substring(0, result.Length > abbrLength ? abbrLength : result.Length) + (result.Length > abbrLength ? "..." : string.Empty);
            }
            return result;
        }

        public static string DelImgStr(string publishInfoType, string html, bool isDraft)
        {
            string result = html;
            if (publishInfoType == ((int)PublishInfoTypeSetting.Short).ToString())
            {
                return result;
            }
            else
            {
                string regexstr = @"<[^>]*>";    //去除所有的标签

                //@"<script[^>]*?>.*?</script >" //去除所有脚本，中间部分也删除

                // string regexstr = @"<img[^>]*>";   //去除图片的正则

                // string regexstr = @"<(?!br).*?>";   //去除所有标签，只剩br

                // string regexstr = @"<table[^>]*?>.*?</table>";   //去除table里面的所有内容

                //string regexstr = @"<(?!img|br|p|/p).*?>";   //去除所有标签，只剩img,br,p



                result = Regex.Replace(result, regexstr, string.Empty, RegexOptions.IgnoreCase);


                result = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + result.Substring(0, result.Length > 200 ? 200 : result.Length) + (result.Length > 200 ? (isDraft ? "..." : "...") : string.Empty);
            }

            return result;
        }

        /// <summary>
        /// 获取本站点绝对Uri
        /// </summary>
        /// <param name="relativePath">相对路径</param>
        /// <returns>Uri地址</returns>
        public static string GetBaseFullUrl(string relativePath)
        {
            string result = string.Empty;
            string baseUrl = WebConfigurationManager.AppSettings["BaseUrl"].ToString();
            if (!string.IsNullOrEmpty(relativePath))
            {

                result = baseUrl + relativePath;
            }
            return result;
        }
        /// <summary>
        /// 获取后台管理站点绝对Uri
        /// </summary>
        /// <param name="relativePath">相对路径</param>
        /// <returns>Uri地址</returns>
        public static string GetManageFullUrl(string relativePath)
        {
            string result = string.Empty;
            string manageUrl = WebConfigurationManager.AppSettings["ManageUrl"].ToString();
            if (!string.IsNullOrEmpty(relativePath))
            {

                result = manageUrl + relativePath;
            }
            return result;
        }
        /// <summary>
        /// 跨域上传
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>路径</returns>
        public static string CrossDomainUpload(string url, string filePath)
        {
            string result = string.Empty;
            WebClient client = new WebClient();
            client.Credentials = CredentialCache.DefaultCredentials;
            try
            {
                byte[] data = client.UploadFile(url, "POST", filePath);
                result = Encoding.UTF8.GetString(data);
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
            return result;

        }
        /// <summary>
        /// 转换文件类型
        /// </summary>
        /// <param name="fileExt">扩展名</param>
        /// <returns>文件类型</returns>
        public static AccFileTypeSetting ConvertAccFileType(string fileExt)
        {
            AccFileTypeSetting result;
            if (fileExt == "doc" || fileExt == "docx")
            {
                result = AccFileTypeSetting.Word;
            }
            else if (fileExt == "xls" || fileExt == "xlsx")
            {
                result = AccFileTypeSetting.Excel;
            }
            else if (fileExt == "ppt" || fileExt == "pptx")
            {
                result = AccFileTypeSetting.PowerPoint;
            }
            else if (fileExt == "pdf")
            {
                result = AccFileTypeSetting.Pdf;
            }
            else if (fileExt == "txt")
            {
                result = AccFileTypeSetting.Text;
            }
            else if (fileExt == "rar" || fileExt == "zip")
            {
                result = AccFileTypeSetting.Accessory;
            }
            else if (fileExt == "jpg" || fileExt == "png" || fileExt == "gif")
            {
                result = AccFileTypeSetting.Photo;
            }
            else if (fileExt == "xml") 
            {
                result = AccFileTypeSetting.Xml;
            }
            else
            {
                throw new NotSupportedException(fileExt + "类型：暂不支持！");
            }
            return result;
        }
    }
}