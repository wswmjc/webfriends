using Miic.Base;
using Miic.Log;
//using SharpCompress.Common;
//using SharpCompress.Reader;
using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;

/// <summary>
/// UnzipService
/// </summary>
[WebService(Namespace = "http://pyq.mictalk.cn/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[ScriptService]
public class CrossFriendsThemeHandleService : System.Web.Services.WebService
{
    private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
    private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
    public CrossFriendsThemeHandleService()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    /// <summary>
    /// 复制文件
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="destPath"></param>
    /// <returns></returns>
    [WebMethod]
    public bool CopyFileTheme(string sourcePath, string destPath)
    {
        bool result = false;
        try
        {
            string dest = HttpContext.Current.Server.MapPath(destPath);
            string source = HttpContext.Current.Server.MapPath(sourcePath);
            File.Copy(source, dest, true);
            result = true;
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
    /// 删除文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    [WebMethod]
    public bool DeleteFileTheme(string path)
    {
        bool result = false;
        try
        {
            if (File.Exists(HttpContext.Current.Server.MapPath(path)))
            {
                File.Delete(HttpContext.Current.Server.MapPath(path));
            }
            result = true;
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
    /// 删除文件夹及其下的所有文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    [WebMethod]
    public bool DeleteDirectoryTheme(string path)
    {
        bool result = false;
        try
        {
            if (Directory.Exists(HttpContext.Current.Server.MapPath(path)))
            {
                Directory.Delete(HttpContext.Current.Server.MapPath(path), true);
            }
            result = true;
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
    /// 解压文件
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="destPath"></param>
    /// <returns></returns>
    //[WebMethod]
    //public bool UnZipTheme(string sourcePath, string destPath)
    //{
    //    bool result = false;
    //    try
    //    {
    //        using (Stream stream = File.OpenRead(HttpContext.Current.Server.MapPath(@"" + sourcePath)))//@"\file\temp\Theme\ThemeCssZip\theme3.zip"
    //        {
    //            var reader = ReaderFactory.Open(stream);
    //            while (reader.MoveToNextEntry())
    //            {
    //                if (!reader.Entry.IsDirectory)
    //                {
    //                    Console.WriteLine(reader.Entry.FilePath);
    //                    reader.WriteEntryToDirectory(HttpContext.Current.Server.MapPath(@"" + destPath), ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);//@"\images\Theme"
    //                }
    //            }
    //            result = true;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Config.IlogicLogService.Write(new LogicLog()
    //        {
    //            AppName = Config.AppName,
    //            ClassName = ClassName,
    //            NamespaceName = NamespaceName,
    //            MethodName = MethodBase.GetCurrentMethod().Name,
    //            Message = ex.Message,
    //            Oper = Config.Oper
    //        });
    //    }
    //    return result;
    //}
    
}
