using Miic.Base;
using Miic.Base.Setting;
using Miic.DB;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Common.Setting;
using Miic.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Miic.Friends.Moments
{
    public partial class PublishInfoDao : RelationCommon<PublishInfo, AccessoryInfo>, IPublishInfo
    {

        bool ICommon<AccessoryInfo>.Insert(AccessoryInfo publishAccessoryInfo)
        {
            Contract.Requires<ArgumentNullException>(publishAccessoryInfo != null, "参数publishAccessoryInfo：不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(publishAccessoryInfo.ID), "参数publishAccessoryInfo.ID：不能为空！");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            try
            {
                result = dbService.Insert(publishAccessoryInfo, out count, out message);
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
            if (result == true)
            {
                List<AccessoryInfo> accessoryList = new List<AccessoryInfo>();
                accessoryList.Add(publishAccessoryInfo);
                InsertCaches(accessoryList);
            }
            return result;
        }

        bool ICommon<AccessoryInfo>.Update(AccessoryInfo publishAccessoryInfo)
        {
            Contract.Requires<ArgumentNullException>(publishAccessoryInfo != null, "参数publishAccessoryInfo:不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(publishAccessoryInfo.ID), "参数publishAccessoryInfo.ID:不能为空，因为是主键");
            int count = 0;
            string message = string.Empty;
            bool result = false;
            try
            {
                result = dbService.Update(publishAccessoryInfo, out count, out message);
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
            if (result == true)
            {
                DeleteCache(o => o.ID == publishAccessoryInfo.ID);
            }
            return result;
        }

        bool ICommon<AccessoryInfo>.Delete(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            AccessoryInfo item = ((ICommon<AccessoryInfo>)this).GetInformation(id);
            MiicCondition publishCondtion = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.PublishID),
                item.PublishID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>());
            bool fileResult = false;
            try
            {
                int accCount = dbService.GetCount<AccessoryInfo>(column, new MiicConditionSingle(publishCondtion), out message);
                try
                {
                    if (!string.IsNullOrEmpty(item.FilePath))
                    {
                        File.Delete(HttpContext.Current.Server.MapPath(item.FilePath));
                        if (item.FileType == ((int)AccFileTypeSetting.Photo).ToString())
                        {
                            File.Delete(HttpContext.Current.Server.MapPath("/file/temp/PublishInfoAcc/Photo/" + Path.GetFileName(item.FilePath)));
                        }
                        else
                        {
                            File.Delete(HttpContext.Current.Server.MapPath("/file/temp/PublishInfoAcc/File/" + Path.GetFileName(item.FilePath)));
                        }
                    }
                    fileResult = true;
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


                if (fileResult == true)
                {
                    if (accCount > 1)
                    {
                        result = dbService.Delete(new AccessoryInfo()
                        {
                            ID = id
                        }, out count, out message);
                    }
                    else if (accCount == 1)
                    {
                        List<string> sqls = new List<string>();
                        sqls.Add(DBService.UpdateSql<PublishInfo>(new PublishInfo()
                        {
                            ID = item.PublishID,
                            HasAcc = ((int)MiicYesNoSetting.No).ToString()
                        }, out message));

                        sqls.Add(DBService.DeleteSql<AccessoryInfo>(new AccessoryInfo()
                        {
                            ID = id
                        }, out message));
                        result = dbService.excuteSqls(sqls, out message);
                    }
                    else {
                        result = true;
                    }
                }
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
            if (result == true)
            {
                DeleteCache(o => o.ID == id);
            }
            return result;
        }

        AccessoryInfo ICommon<AccessoryInfo>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            AccessoryInfo result = null;
            string message = string.Empty;
            try
            {
                result = subitems.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new AccessoryInfo
                    {
                        ID = id
                    }, out message);
                    if (result != null)
                    {
                        List<AccessoryInfo> accessoryList = new List<AccessoryInfo>();
                        accessoryList.Add(result);
                        InsertCaches(accessoryList);
                    }
                }
                else
                {
                    string serializer = Config.Serializer.Serialize(result);
                    result = Config.Serializer.Deserialize<AccessoryInfo>(serializer);
                }
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
        /// 根据微博ID获取所有微博附件列表
        /// </summary>
        /// <param name="publishID">微博ID</param>
        /// <returns>微博附件列表</returns>
        public List<AccessoryInfo> GetAccessoryList(string publishID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(publishID), "参数publishID:不能为空");
            List<AccessoryInfo> result = new List<AccessoryInfo>();
            MiicCondition publishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.PublishID),
                publishID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            string message = string.Empty;
            MiicColumn allColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>());
            MiicColumnCollections columns = new MiicColumnCollections();
            columns.Add(allColumn);
            MiicConditionSingle condition = new MiicConditionSingle(publishIDCondition);
            try
            {
                DataTable dt = dbService.GetInformations<AccessoryInfo>(columns, condition, out message);
                if (dt.Rows.Count != 0)
                {
                    subitems.RemoveAll(o => o.PublishID == publishID);
                    foreach (var item in dt.AsEnumerable())
                    {
                        result.Add(new AccessoryInfo()
                        {
                            ID = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID)].ToString(),
                            PublishID = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.PublishID)].ToString(),
                            FileName = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileName)].ToString(),
                            FilePath = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FilePath)].ToString(),
                            UploadTime = (DateTime?)item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, DateTime?>(o => o.UploadTime)],
                            FileType = item[Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileType)].ToString()
                        });
                    }
                    subitems.AddRange(result);
                }
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
    }
}
