using Miic.Base;
using Miic.Base.Setting;
using Miic.Common;
using Miic.DB;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Common;
using Miic.Friends.Common.Setting;
using Miic.Friends.Community.Behavior;
using Miic.Friends.Notice;
using Miic.Log;
using Miic.Manage.User;
using Miic.Manage.User.Setting;
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

namespace Miic.Friends.Community
{
    public partial class PublishInfoDao : RelationCommon<PublishInfo, AccessoryInfo>, IPublishInfo
    {
        private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
        private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;

        public PublishInfoDao() { }

        /// <summary>
        /// 将临时存储路径替换为指定朋友圈的路径
        /// </summary>
        /// <param name="path">临时存储路径</param>
        /// <returns>指定朋友圈的存储路径</returns>
        private string GetRealSavePath(string path)
        {
            string savePath = string.Empty;
            DateTime now = DateTime.Now;
            string year = now.Year.ToString();
            string month = now.Month < 10 ? "0" + now.Month.ToString() : now.Month.ToString();
            savePath = path.Replace("PublishInfoAcc", "Community");
            savePath = savePath.Replace("File", "File" + "/" + year + month);
            savePath = savePath.Replace("Photo", "Photo" + "/" + year + month);
            //如果月份文件夹不存在则创建文件夹
            string directoryPath = Path.GetDirectoryName(HttpContext.Current.Server.MapPath(savePath));
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
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
            return savePath;
        }

        /// <summary>
        /// 根据行业圈子ID删除发布信息缓存
        /// </summary>
        /// <param name="communityID">行业圈子ID</param>
        /// <returns>Yes/No</returns>
        //public static bool DeleteCacheByCommunityID(string communityID) 
        //{
        //    bool result = false;
        //    try
        //    {
        //        List<PublishInfo> cachePublishList = PublishInfoDao.items.FindAll(o => o.CommunityID == communityID);
        //        foreach (var item in cachePublishList.AsEnumerable())
        //        {
        //            PublishInfoDao.DeleteCache(true, o => o.ID == item.ID, o => o.PublishID == item.ID);
        //        }
        //        result = true;
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
        bool ICommon<PublishInfo>.Insert(PublishInfo publishInfo)
        {
            Contract.Requires<ArgumentNullException>(publishInfo != null, "参数publishInfo：不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(publishInfo.ID), "参数publishInfo.ID：不能为空！");
            bool result = false;
            string message = string.Empty;
            List<string> sqls = new List<string>();
            /*------------------------------初始化值-----------------------------------*/

            //浏览总数
            publishInfo.BrowseNum = 0;
            //点赞数
            publishInfo.PraiseNum = 0;
            //踩数
            publishInfo.TreadNum = 0;
            //转发数
            publishInfo.TransmitNum = 0;
            //被评论数
            publishInfo.CommentNum = 0;
            //
            publishInfo.ReportNum = 0;
            //收藏总数
            publishInfo.CollectNum = 0;
            //微博是否有附件
            publishInfo.HasAcc = ((int)MiicYesNoSetting.No).ToString();
            /*------------------------------初始化值-----------------------------------*/
            sqls.Add(DBService.InsertSql<PublishInfo>(publishInfo, out message));

            //积分
            if (publishInfo.EditStatus == ((int)MiicYesNoSetting.No).ToString())
            {
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = publishInfo.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Publish).ToString(),
                    Score = ScoreConfig.Score.PublishScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = publishInfo.CreaterID,
                    UserName = publishInfo.CreaterName
                }, out message));
            }
            try
            {
                result = dbService.excuteSqls(sqls, out message);
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
                InsertCache(publishInfo);
            }
            return result;
        }

        bool ICommon<PublishInfo>.Update(PublishInfo publishInfo)
        {
            Contract.Requires<ArgumentNullException>(publishInfo != null, "参数publishInfo:不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(publishInfo.ID), "参数publishInfo.ID:不能为空，因为是主键");
            string message = string.Empty;
            bool result = false;
            List<string> sqls = new List<string>();
            PublishInfo temp = ((ICommon<PublishInfo>)this).GetInformation(publishInfo.ID);
            if (temp.EditStatus == ((int)MiicYesNoSetting.No).ToString() && publishInfo.EditStatus == ((int)MiicYesNoSetting.Yes).ToString())
            {
                //积分
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = publishInfo.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Publish).ToString(),
                    Score = -1 * ScoreConfig.Score.PublishScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = temp.CreaterID,
                    UserName = temp.CreaterName
                }, out message));
            }

            if (temp.EditStatus == ((int)MiicYesNoSetting.Yes).ToString() && publishInfo.EditStatus == ((int)MiicYesNoSetting.No).ToString())
            {
                //积分
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = publishInfo.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Publish).ToString(),
                    Score = ScoreConfig.Score.PublishScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = temp.CreaterID,
                    UserName = temp.CreaterName
                }, out message));
            }

            sqls.Add(DBService.UpdateSql<PublishInfo>(publishInfo, out message));

            try
            {
                result = dbService.excuteSqls(sqls, out message);
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
                DeleteCache(true, o => o.ID == publishInfo.ID, o => o.PublishID == publishInfo.ID);
            }
            return result;
        }

        bool ICommon<PublishInfo>.Delete(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            bool result = false;
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            string message3 = string.Empty;
            string messagex = string.Empty;
            List<string> sqls = new List<string>();
            List<AccessoryInfo> accs = GetAccessoryList(id);
            if (accs.Count > 0)
            {
                MiicCondition idCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.PublishID),
                    id,
                    DbType.String,
                    MiicDBOperatorSetting.Equal);
                MiicConditionSingle condition = new MiicConditionSingle(idCondition);
                sqls.Add(DBService.DeleteConditionSql<AccessoryInfo>(condition, out message2));
            }
            sqls.Add(DBService.DeleteSql<PublishInfo>(new PublishInfo()
            {
                ID = id
            }, out message1));
            MiicCondition publishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.PublishID),
                id,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            MiicConditionSingle relation = new MiicConditionSingle(publishIDCondition);
            sqls.Add(DBService.DeleteConditionSql<PublishLabelRelation>(relation, out messagex));
            //提醒级联
            MiicCondition noticeIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<NoticeInfo, string>(o => o.PublishID),
               id,
               DbType.String,
               MiicDBOperatorSetting.Equal);
            MiicConditionSingle noticeCondition = new MiicConditionSingle(noticeIDCondition);
            sqls.Add(DBService.DeleteConditionSql<NoticeInfo>(noticeCondition, out message3));
            PublishInfo temp = ((ICommon<PublishInfo>)this).GetInformation(id);
            //积分
            if (temp.EditStatus == ((int)MiicYesNoSetting.No).ToString())
            {
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = temp.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Publish).ToString(),
                    Score = -1 * ScoreConfig.Score.PublishScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = temp.CreaterID,
                    UserName = temp.CreaterName
                }, out message));
            }
            try
            {
                result = dbService.excuteSqls(sqls, out message);
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
                if (accs.Count > 0)
                {
                    foreach (AccessoryInfo acc in accs.AsEnumerable())
                    {
                        File.Delete(HttpContext.Current.Server.MapPath(acc.FilePath));
                        if (acc.FileType == ((int)AccFileTypeSetting.Photo).ToString())
                        {
                            File.Delete(HttpContext.Current.Server.MapPath("/file/temp/PublishInfoAcc/Photo/" + Path.GetFileName(acc.FilePath)));
                        }
                        else
                        {
                            File.Delete(HttpContext.Current.Server.MapPath("/file/temp/PublishInfoAcc/File/" + Path.GetFileName(acc.FilePath)));
                        }
                    }
                    DeleteCache(true, o => o.ID == id, o => o.PublishID == id);
                }
                else
                {
                    DeleteCache(false, o => o.ID == id);
                }
                PraiseInfoDao.DeleteCacheByPublishID(id);
                TreadInfoDao.DeleteCacheByPublishID(id);
                CollectInfoDao.DeleteCacheByPublishID(id);
                BrowseInfoDao.DeleteCacheByPublishID(id);
                CommentInfoDao.DeleteCacheByPublishID(id);
            }

            return result;
        }

        PublishInfo ICommon<PublishInfo>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            PublishInfo result = null;
            string message = string.Empty;
            try
            {
                result = items.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new PublishInfo
                    {
                        ID = id
                    }, out message);
                    if (result != null)
                    {
                        InsertCache(result);
                    }
                }
                else
                {
                    string serializer = Config.Serializer.Serialize(result);
                    result = Config.Serializer.Deserialize<PublishInfo>(serializer);
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
        /// 根据ID获取详细信息内容
        /// </summary>
        /// <param name="ID">信息ID</param>
        /// <returns>详细信息</returns>
        public DataTable GetDetailPublishInfo(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            string message = string.Empty;
            DataTable result = new DataTable();
            MiicCondition idCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<PublishInfo, string>(o => o.ID),
                id,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            MiicConditionSingle condition = new MiicConditionSingle(idCondition);
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID),
               Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.PublishID),
               MiicDBOperatorSetting.Equal,
               MiicDBRelationSetting.LeftJoin);
            MiicColumnCollections column = new MiicColumnCollections();
            MiicColumn microColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>());
            column.Add(microColumn);
            MiicColumn microAccessoryIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                string.Empty,
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID),
                "CommunityPublishAccessoryInfoID");
            column.Add(microAccessoryIDColumn);
            MiicColumn fileNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileName));
            column.Add(fileNameColumn);
            MiicColumn filePathColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FilePath));
            column.Add(filePathColumn);
            MiicColumn uploadTime = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, DateTime?>(o => o.UploadTime));
            column.Add(uploadTime);
            MiicColumn fileTypeColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileType));
            column.Add(fileTypeColumn);
            try
            {
                result = dbService.GetInformations(column, relation, condition, out message);
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
        /// 新增行业圈子信息
        /// </summary>
        /// <param name="publishInfo">信息</param>
        /// <param name="accessoryInfos">信息附件，可为空</param>
        /// <param name="simpleLabelViews">信息标签，可为空</param>
        /// <param name="noticeUserView">提醒人，可为空</param>
        /// <returns></returns>
        public bool Insert(PublishInfo publishInfo, List<AccessoryInfo> accessoryInfos, List<SimpleLabelView> simpleLabelViews, NoticeUserView noticeUserView)
        {
            Contract.Requires<ArgumentNullException>(publishInfo != null, "参数publishInfo:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(publishInfo.ID), "参数publishInfo.ID:不能为空");
            bool result = false;
            List<string> sqls = new List<string>();
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            string message3 = string.Empty;
            string message4 = string.Empty;
            bool fileResult = false;

            /*------------------------------信息初始化值-----------------------------------*/
            //浏览总数
            publishInfo.BrowseNum = 0;
            //点赞数
            publishInfo.PraiseNum = 0;
            //踩数
            publishInfo.TreadNum = 0;
            //转发数
            publishInfo.TransmitNum = 0;
            //被评论数
            publishInfo.CommentNum = 0;
            //被举报数目
            publishInfo.ReportNum = 0;
            //收藏总数
            publishInfo.CollectNum = 0;
            //微博是否有附件
            if (accessoryInfos == null || accessoryInfos.Count == 0)
            {
                publishInfo.HasAcc = ((int)MiicYesNoSetting.No).ToString();
            }
            else
            {
                publishInfo.HasAcc = ((int)MiicYesNoSetting.Yes).ToString();
            }
            /*------------------------------信息初始化值-----------------------------------*/

            if (accessoryInfos == null && simpleLabelViews == null && noticeUserView == null)
            {
                result = ((ICommon<PublishInfo>)this).Insert(publishInfo);
            }
            else
            {
                //插入信息主体
                sqls.Add(DBService.InsertSql(publishInfo, out message1));

                //积分
                if (publishInfo.EditStatus == ((int)MiicYesNoSetting.No).ToString())
                {
                    sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                    {
                        ID = Guid.NewGuid().ToString(),
                        BusinessID = publishInfo.ID,
                        CreateTime = DateTime.Now,
                        GetWay = ((int)GetWayTypeSetting.Publish).ToString(),
                        Score = ScoreConfig.Score.PublishScore,
                        ServiceID = ScoreConfig.ServiceID,
                        UserID = publishInfo.CreaterID,
                        UserName = publishInfo.CreaterName
                    }, out message));
                }
                //检测附件
                if (accessoryInfos != null && accessoryInfos.Count != 0)
                {
                    foreach (var item in accessoryInfos.AsEnumerable())
                    {
                        item.FilePath = GetRealSavePath(item.FilePath);
                        sqls.Add(DBService.InsertSql(item, out message2));
                    }

                    try
                    {
                        foreach (var item in accessoryInfos)
                        {
                            string dest = HttpContext.Current.Server.MapPath(item.FilePath);
                            string source = string.Empty;
                            if (item.FileType == ((int)AccFileTypeSetting.Photo).ToString())
                            {
                                source = HttpContext.Current.Server.MapPath("/file/temp/PublishInfoAcc/Photo/" + Path.GetFileName(dest));
                            }
                            else
                            {
                                source = HttpContext.Current.Server.MapPath("/file/temp/PublishInfoAcc/File/" + Path.GetFileName(dest));
                            }
                            File.Copy(source, dest, true);
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
                }
                else
                {
                    fileResult = true;
                }

                //检测标签
                if (simpleLabelViews != null && simpleLabelViews.Count != 0)
                {
                    foreach (var item in simpleLabelViews.AsEnumerable())
                    {
                        if (publishInfo.PublishType == ((int)PublishInfoTypeSetting.Long).ToString())
                        {
                            sqls.Add(DBService.InsertSql(new PublishLabelRelation()
                            {
                                LabelID = item.LabelID,
                                LabelName = item.LabelName,
                                PublishID = publishInfo.ID,
                                PublishName = publishInfo.Title,
                                CommunityID = item.CommunityID,
                                Valid = ((int)MiicValidTypeSetting.Valid).ToString()
                            }, out message3));
                        }
                        else
                        {
                            sqls.Add(DBService.InsertSql(new PublishLabelRelation()
                            {
                                LabelID = item.LabelID,
                                LabelName = item.LabelName,
                                PublishID = publishInfo.ID,
                                PublishName = publishInfo.Content,
                                CommunityID = item.CommunityID,
                                Valid = ((int)MiicValidTypeSetting.Valid).ToString()
                            }, out message3));
                        }
                    }
                }

                //检测提醒人
                if (noticeUserView != null && noticeUserView.Noticers != null)
                {
                    //@人员
                    foreach (var item in noticeUserView.Noticers.AsEnumerable())
                    {
                        sqls.Add(DBService.InsertSql<NoticeInfo>(new NoticeInfo()
                        {
                            ID = Guid.NewGuid().ToString(),
                            NoticerID = item.UserID,
                            NoticerName = item.UserName,
                            Source = ((int)noticeUserView.NoticeSource).ToString(),
                            NoticeType = ((int)noticeUserView.NoticeType).ToString(),
                            PublisherID = publishInfo.CreaterID,
                            PublisherName = publishInfo.CreaterName,
                            PublishID = publishInfo.ID,
                            PublishTime = publishInfo.PublishTime,
                            ReadStatus = ((int)MiicReadStatusSetting.UnRead).ToString()
                        }, out message4));
                    }
                }

                try
                {
                    if (fileResult == true)
                    {
                        result = dbService.excuteSqls(sqls, out message);
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
            }
            return result;
        }

        /// <summary>
        /// 修改行业圈子信息
        /// </summary>
        /// <param name="publishInfo">信息</param>
        /// <param name="accessoryInfos">信息附件，可为空</param>
        /// <param name="simpleLabelViews">信息标签，可为空</param>
        /// <param name="removeSimpleAccessoryViews">删除附件，可为空</param>
        /// <param name="removeSimpleLabelViewIDs">删除标签，可为空</param>
        /// <param name="noticeUserView">提醒人，可为空</param>
        /// <returns></returns>
        public bool Update(PublishInfo publishInfo, List<AccessoryInfo> accessoryInfos, List<SimpleLabelView> simpleLabelViews, List<SimpleAccessoryView> removeSimpleAccessoryViews, List<string> removeSimpleLabelViewIDs, NoticeUserView noticeUserView)
        {
            Contract.Requires<ArgumentNullException>(publishInfo != null, "参数publishInfo:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(publishInfo.ID), "参数publishInfo.ID:不能为空");
            bool result = false;
            List<string> sqls = new List<string>();
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            string message3 = string.Empty;
            string message4 = string.Empty;
            string message5 = string.Empty;
            string message6 = string.Empty;
            bool fileResult = false;

            PublishInfo tempPublishInfo = ((ICommon<PublishInfo>)this).GetInformation(publishInfo.ID);

            if (tempPublishInfo.EditStatus == ((int)MiicYesNoSetting.No).ToString() && publishInfo.EditStatus == ((int)MiicYesNoSetting.Yes).ToString())
            {
                //积分
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = publishInfo.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Publish).ToString(),
                    Score = -1 * ScoreConfig.Score.PublishScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = tempPublishInfo.CreaterID,
                    UserName = tempPublishInfo.CreaterName
                }, out message));
            }

            if (tempPublishInfo.EditStatus == ((int)MiicYesNoSetting.Yes).ToString() && publishInfo.EditStatus == ((int)MiicYesNoSetting.No).ToString())
            {
                //积分
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = publishInfo.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Publish).ToString(),
                    Score = ScoreConfig.Score.PublishScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = tempPublishInfo.CreaterID,
                    UserName = tempPublishInfo.CreaterName
                }, out message));
            }

            List<AccessoryInfo> tempAccessoryList = this.GetAccessoryList(publishInfo.ID);
            if (removeSimpleAccessoryViews != null && removeSimpleAccessoryViews.Count != 0)
            {
                if (tempAccessoryList.Count == removeSimpleAccessoryViews.Count && (accessoryInfos == null || accessoryInfos.Count == 0))
                {
                    publishInfo.HasAcc = ((int)MiicYesNoSetting.No).ToString();
                }
                //删除附件
                foreach (var item in removeSimpleAccessoryViews)
                {
                    sqls.Add(DBService.DeleteSql<AccessoryInfo>(new AccessoryInfo()
                    {
                        ID = item.ID
                    }, out message1));
                }
            }

            //更新信息
            sqls.Add(DBService.UpdateSql<PublishInfo>(publishInfo, out message2));

            //检测附件
            if (accessoryInfos != null && accessoryInfos.Count != 0)
            {
                foreach (var item in accessoryInfos.AsEnumerable())
                {
                    item.FilePath = GetRealSavePath(item.FilePath);
                    sqls.Add(DBService.InsertSql(item, out message3));
                }

                try
                {
                    foreach (var item in accessoryInfos)
                    {
                        string dest = HttpContext.Current.Server.MapPath(item.FilePath);
                        string source = string.Empty;
                        if (item.FileType == ((int)AccFileTypeSetting.Photo).ToString())
                        {
                            source = HttpContext.Current.Server.MapPath("/file/temp/PublishInfoAcc/Photo/" + Path.GetFileName(dest));
                        }
                        else
                        {
                            source = HttpContext.Current.Server.MapPath("/file/temp/PublishInfoAcc/File/" + Path.GetFileName(dest));
                        }
                        File.Copy(source, dest, true);
                    }

                    if (removeSimpleAccessoryViews != null && removeSimpleAccessoryViews.Count != 0)
                    {
                        foreach (var item in removeSimpleAccessoryViews)
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
            }
            else
            {
                fileResult = true;
            }

            //检测标签
            if (simpleLabelViews != null && simpleLabelViews.Count != 0)
            {
                foreach (var item in simpleLabelViews.AsEnumerable())
                {
                    if (tempPublishInfo.PublishType == ((int)PublishInfoTypeSetting.Long).ToString())
                    {
                        sqls.Add(DBService.InsertSql(new PublishLabelRelation()
                        {
                            LabelID = item.LabelID,
                            LabelName = item.LabelName,
                            PublishID = publishInfo.ID,
                            PublishName = !string.IsNullOrEmpty(publishInfo.Title) ? tempPublishInfo.Title : publishInfo.Title,
                            CommunityID = item.CommunityID,
                            Valid = ((int)MiicValidTypeSetting.Valid).ToString()
                        }, out message4));
                    }
                    else
                    {
                        sqls.Add(DBService.InsertSql(new PublishLabelRelation()
                        {
                            LabelID = item.LabelID,
                            LabelName = item.LabelName,
                            PublishID = publishInfo.ID,
                            PublishName = !string.IsNullOrEmpty(publishInfo.Content) ? tempPublishInfo.Content : publishInfo.Content,
                            CommunityID = item.CommunityID,
                            Valid = ((int)MiicValidTypeSetting.Valid).ToString()
                        }, out message4));
                    }
                }
            }

            //检测删除标签
            if (removeSimpleLabelViewIDs != null && removeSimpleLabelViewIDs.Count != 0)
            {
                foreach (var item in removeSimpleLabelViewIDs)
                {
                    sqls.Add(DBService.DeleteSql<PublishLabelRelation>(new PublishLabelRelation()
                    {
                        PublishID = publishInfo.ID,
                        LabelID = item
                    }, out message5));
                }
            }

            //检测提醒人
            if (noticeUserView != null && noticeUserView.Noticers != null)
            {
                //@人员
                foreach (var item in noticeUserView.Noticers.AsEnumerable())
                {
                    sqls.Add(DBService.InsertSql<NoticeInfo>(new NoticeInfo()
                    {
                        ID = Guid.NewGuid().ToString(),
                        NoticerID = item.UserID,
                        NoticerName = item.UserName,
                        Source = ((int)noticeUserView.NoticeSource).ToString(),
                        NoticeType = ((int)noticeUserView.NoticeType).ToString(),
                        PublisherID = tempPublishInfo.CreaterID,
                        PublisherName = tempPublishInfo.CreaterName,
                        PublishID = tempPublishInfo.ID,
                        PublishTime = publishInfo.PublishTime == null ? tempPublishInfo.PublishTime : publishInfo.PublishTime,
                        ReadStatus = ((int)MiicReadStatusSetting.UnRead).ToString()
                    }, out message6));
                }
            }

            try
            {
                if (fileResult == true)
                {
                    result = dbService.excuteSqls(sqls, out message);
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
        /// 设置已经发布的微博上下线
        /// </summary>
        /// <param name="ID">上下线状态视图</param>
        /// <returns>Yes/No</returns>
        public bool SetEditStatus(EditStatusView editStatusView)
        {
            Contract.Requires<ArgumentNullException>(editStatusView != null, "参数editStatusView:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(editStatusView.ID), "参数editStatusView.ID:不能为空");
            bool result = false;
            string message = string.Empty;
            int count = 0;
            try
            {
                result = dbService.Update<PublishInfo>(new PublishInfo()
                {
                    ID = editStatusView.ID,
                    UpdateTime = DateTime.Now,
                    EditStatus = ((int)editStatusView.EditStatus).ToString()
                }, out count, out message);
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
                DeleteCache(true, o => o.ID == editStatusView.ID, o => o.PublishID == editStatusView.ID);
            }
            return result;
        }

        /// <summary>
        /// 获取行业圈子某标签内发布的所有信息
        /// </summary>
        /// <param name="dateView">日期视图</param>
        /// <param name="page">分页</param>
        /// <returns>发布信息列表</returns>
        public DataTable GetCommunityPublishInfos(CommunityDateView dateView, MiicPage page)
        {
            Contract.Requires<ArgumentNullException>(dateView != null, "参数dateView:不能为空");
            Contract.Requires<ArgumentNullException>(page != null, "参数page:不能为空");
            DataTable result = new DataTable();
            MiicColumnCollections column = new MiicColumnCollections();
            string message = string.Empty;
            try
            {
                Dictionary<String, String> paras = new Dictionary<String, String>();
                paras.Add("USER_ID", dateView.UserID);
                paras.Add("YEAR", dateView.Year);
                paras.Add("MONTH", dateView.Month);
                paras.Add("COMMUNITY_ID", dateView.CommunityID);
                paras.Add("LABEL_ID", dateView.LabelID);
                paras.Add("PAGE_START", page.pageStart);
                paras.Add("PAGE_END", page.pageEnd);
                string storeProcedureName = "GetCommunityLabelPublishInfoSearch";
                result = dbService.QueryStoredProcedure<string>(storeProcedureName, paras, out message);
            }
            catch (Exception ex)
            {
                LogicLog log = new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                };
                Config.IlogicLogService.Write(log);
            }
            return result;
        }

        /// <summary>
        /// 获取行业圈子发布信息数
        /// </summary>
        /// <param name="dateView">日期视图</param>
        /// <returns>发布信息列表数</returns>
        public int GetCommunityPublishCount(CommunityDateView dateView)
        {
            Contract.Requires<ArgumentNullException>(dateView != null, "参数dateView:不能为空");
            int result = 0;
            string message = string.Empty;
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<PublishLabelRelation>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.PublishID),
                Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID), MiicDBOperatorSetting.Equal, MiicDBRelationSetting.LeftJoin);

            MiicConditionCollections condition = dateView.vistor(this);

            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishLabelRelation>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.PublishID));

            try
            {
                result = dbService.GetCount(column, relation, condition, out message);
            }
            catch (Exception ex)
            {
                LogicLog log = new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                };
                Config.IlogicLogService.Write(log);
            }
            return result;
        }
        /// <summary>
        /// 获取自己最早发布的行业圈子内的信息
        /// </summary>
        /// <param name="top">条目数，默认为1</param>
        /// <param name="isSelf">是否是自己发布的，默认为true</param>
        /// <returns>发布信息列表</returns>
        public DataTable GetOldestCommunityPubilishInfos(CommunityTopView topView)
        {
            Contract.Requires<ArgumentOutOfRangeException>(topView != null, "参数topView:不能为空");
            Contract.Requires<ArgumentOutOfRangeException>(topView.Top > 0, "参数topView.Top:必须为正整数");
            DataTable result = new DataTable();
            MiicColumnCollections column = new MiicColumnCollections(new MiicTop(topView.Top));
            string message = string.Empty;
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition communityIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.CommunityID),
               topView.CommunityID,
                DbType.String,
                 MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, communityIDCondition));
            MiicCondition labelIDIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelID),
                topView.LabelID,
                 DbType.String,
                  MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(labelIDIDCondition));
            MiicCondition validCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.Valid),
                ((int)MiicValidTypeSetting.Valid).ToString(),
                 DbType.String,
                  MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(validCondition));
            MiicCondition editStatusCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.EditStatus),
                                                      ((int)MiicYesNoSetting.No).ToString(),
                                                      DbType.String,
                                                      MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(editStatusCondition));

            MiicFriendRelation publishRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.PublishID),
              Config.Attribute.GetSqlTableNameByClassName<PublishInfoWithUserInfo>(),
              Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfoWithUserInfo, string>(o => o.ID),
              MiicDBOperatorSetting.Equal,
              MiicDBRelationSetting.InnerJoin);

            MiicFriendRelation accRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.PublishID),
              Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
              Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.PublishID),
              MiicDBOperatorSetting.Equal,
              MiicDBRelationSetting.LeftJoin);

            //点赞表
            MiicFriendRelation praiseRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.PublishID),
                Config.Attribute.GetSqlTableNameByClassName<PraiseInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PraiseInfo, string>(o => o.PublishID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.LeftJoin);
            //点踩表
            MiicFriendRelation treadRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.PublishID),
                Config.Attribute.GetSqlTableNameByClassName<TreadInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<TreadInfo, string>(o => o.PublishID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.LeftJoin);
            //举报表
            MiicFriendRelation reportRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.PublishID),
                Config.Attribute.GetSqlTableNameByClassName<ReportInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfo, string>(o => o.PublishID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.LeftJoin);
            //收藏表
            MiicFriendRelation collectRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.PublishID),
                Config.Attribute.GetSqlTableNameByClassName<CollectInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.PublishID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.LeftJoin);

            List<MiicFriendRelation> relations = new List<MiicFriendRelation>();
            relations.Add(publishRelation);
            relations.Add(accRelation);
            relations.Add(praiseRelation);
            relations.Add(treadRelation);
            relations.Add(reportRelation);
            relations.Add(collectRelation);

            MiicRelationCollections relation = new MiicRelationCollections(Config.Attribute.GetSqlTableNameByClassName<PublishLabelRelation>(), relations);

            MiicColumn publishInfoAllColumns = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfoWithUserInfo>());
            column.Add(publishInfoAllColumns);

            MiicColumn communityIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishLabelRelation>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.CommunityID));
            column.Add(communityIDColumn);
            MiicColumn publishIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishLabelRelation>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.PublishID));
            column.Add(publishIDColumn);
            MiicColumn validColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishLabelRelation>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.Valid));
            column.Add(validColumn);
            MiicColumn labelNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishLabelRelation>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelName));
            column.Add(labelNameColumn);
            MiicColumn labelIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishLabelRelation>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelID));
            column.Add(labelIDColumn);

            MiicColumn publishAccessoryIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                string.Empty,
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID),
                "CommunityPublishAccessoryInfoID");
            column.Add(publishAccessoryIDColumn);
            MiicColumn fileNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileName));
            column.Add(fileNameColumn);
            MiicColumn filePathColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FilePath));
            column.Add(filePathColumn);
            MiicColumn uploadTime = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, DateTime?>(o => o.UploadTime));
            column.Add(uploadTime);
            MiicColumn fileTypeColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileType));
            column.Add(fileTypeColumn);

            //点赞人员表
            MiicColumn praiserIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PraiseInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PraiseInfo, string>(o => o.PraiserID));
            column.Add(praiserIDColumn);
            MiicColumn praiserNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PraiseInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PraiseInfo, string>(o => o.PraiserName));
            column.Add(praiserNameColumn);
            //点踩人员表
            MiicColumn treaderIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<TreadInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<TreadInfo, string>(o => o.TreaderID));
            column.Add(treaderIDColumn);
            MiicColumn treaderNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<TreadInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<TreadInfo, string>(o => o.TreaderName));
            column.Add(treaderNameColumn);
            //举报人员表
            MiicColumn reporterIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<ReportInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfo, string>(o => o.ReporterID));
            column.Add(reporterIDColumn);
            MiicColumn reporterNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<ReportInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<ReportInfo, string>(o => o.ReporterName));
            column.Add(reporterNameColumn);
            //收藏人员表
            MiicColumn collectorIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CollectInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.CollectorID));
            column.Add(collectorIDColumn);
            MiicColumn collectorNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CollectInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.CollectorName));
            column.Add(collectorNameColumn);
            MiicColumn collectorValidColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CollectInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CollectInfo, string>(o => o.CollectValid));
            column.Add(collectorValidColumn);
            try
            {
                result = dbService.GetInformations(column, relation, condition, out message);
            }
            catch (Exception ex)
            {
                LogicLog log = new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                };
                Config.IlogicLogService.Write(log);
            }
            return result;
        }

        /// <summary>
        /// 获取年份列表
        /// </summary>
        /// <param name="dateView"></param>
        /// <returns></returns>
        public List<string> GetCommunityPublishInfosYearList(CommunityDateView dateView)
        {
            Contract.Requires<ArgumentNullException>(dateView != null, "参数dateView:不能为空");
            List<string> result = new List<string>();
            MiicColumnCollections column = new MiicColumnCollections(new MiicDistinct());
            string message = string.Empty;

            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition communityIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.CommunityID),
                dateView.CommunityID,
                 DbType.String,
                  MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, communityIDCondition));
            MiicCondition labelIDIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelID),
                dateView.LabelID,
                 DbType.String,
                  MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(labelIDIDCondition));
            MiicCondition validCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.Valid),
                ((int)MiicValidTypeSetting.Valid).ToString(),
                 DbType.String,
                  MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(validCondition));
            MiicCondition editStatusCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.EditStatus),
                                                      ((int)MiicYesNoSetting.No).ToString(),
                                                      DbType.String,
                                                      MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(editStatusCondition));

            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<PublishLabelRelation>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.PublishID),
                Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID), MiicDBOperatorSetting.Equal, MiicDBRelationSetting.LeftJoin);

            //默认时间倒序排列
            List<MiicOrderBy> order = new List<MiicOrderBy>();
            order.Add(new MiicOrderBy()
            {
                Desc = true,
                PropertyName = MiicSimpleDateTimeFunction.YearWithTableFunc<PublishInfo, DateTime?>(o => o.PublishTime)
            });
            condition.order = order;
            MiicColumn dateColumn = new MiicColumn(true, MiicSimpleDateTimeFunction.YearWithTableFunc<PublishInfo, DateTime?>(o => o.PublishTime));
            column.Add(dateColumn);
            try
            {
                DataTable dt = dbService.GetInformations(column, relation, condition, out message);
                if (dt.Rows.Count != 0)
                {
                    foreach (var dr in dt.AsEnumerable())
                    {
                        result.Add(dr[0].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LogicLog log = new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                };
                Config.IlogicLogService.Write(log);
            }

            return result;
        }
        /// <summary>
        /// 获取月份列表
        /// </summary>
        /// <param name="dateView"></param>
        /// <returns></returns>
        public List<string> GetCommunityPublishInfosMonthList(CommunityDateView dateView)
        {
            Contract.Requires<ArgumentNullException>(dateView != null, "参数dateView:不能为空");
            List<string> result = new List<string>();
            MiicColumnCollections column = new MiicColumnCollections(new MiicDistinct());
            string message = string.Empty;
            MiicConditionCollections condition = dateView.vistor(this);

            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<PublishLabelRelation>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.PublishID),
                Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID), MiicDBOperatorSetting.Equal, MiicDBRelationSetting.LeftJoin);

            //默认时间倒序排列
            List<MiicOrderBy> order = new List<MiicOrderBy>();
            order.Add(new MiicOrderBy()
            {
                Desc = true,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.PublishTime)
            });
            condition.order = order;

            MiicColumn dateColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.PublishTime));
            column.Add(dateColumn);
            DataTable dt = new DataTable();
            try
            {
                dt = dbService.GetInformations(column, relation, condition, out message);
                if (dt.Rows.Count != 0)
                {
                    foreach (var dr in dt.AsEnumerable())
                    {
                        result.Add(((DateTime)dr[Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.PublishTime)]).Month.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LogicLog log = new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                };
                Config.IlogicLogService.Write(log);
            }

            return result.Distinct().ToList();
        }

        /// <summary>
        /// 搜索某人草稿列表
        /// </summary>
        /// <param name="keywordView">关键字视图</param>
        /// <param name="page">分页项</param>
        /// <returns>某人草稿列表</returns>
        public DataTable GetDraftInfos(DraftSearchView keywordView, MiicPage page)
        {
            Contract.Requires<ArgumentNullException>(keywordView != null, "参数keywordView:不能为空");
            DataTable result = new DataTable();
            MiicColumnCollections column = new MiicColumnCollections();
            List<MiicOrderBy> orders = new List<MiicOrderBy>();
            string message = string.Empty;
            MiicConditionCollections condition = keywordView.visitor(this);

            MiicOrderBy createTimeOrder = new MiicOrderBy()
            {
                Desc = true,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, DateTime?>(o => o.CreateTime)
            };
            orders.Add(createTimeOrder);

            MiicOrderBy titleOrder = new MiicOrderBy()
            {
                Desc = true,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.Title)
            };
            orders.Add(titleOrder);

            condition.order = orders;

            MiicFriendRelation labelRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID),
              Config.Attribute.GetSqlTableNameByClassName<PublishLabelRelation>(),
              Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.PublishID),
              MiicDBOperatorSetting.Equal,
              MiicDBRelationSetting.LeftJoin);

            MiicFriendRelation accRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID),
              Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
              Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.PublishID),
              MiicDBOperatorSetting.Equal,
              MiicDBRelationSetting.LeftJoin);


            MiicFriendRelation userRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.CreaterID),
                Config.Attribute.GetSqlTableNameByClassName<Miic.Manage.User.SimplePersonUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.UserID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.LeftJoin);



            List<MiicFriendRelation> relations = new List<MiicFriendRelation>();
            relations.Add(labelRelation);
            relations.Add(accRelation);
            relations.Add(userRelation);

            MiicRelationCollections relation = new MiicRelationCollections(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(), relations);

            MiicColumn microPublishInfoAllColumns = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>());
            column.Add(microPublishInfoAllColumns);

            MiicColumn communityIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishLabelRelation>(),
              Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.CommunityID));
            column.Add(communityIDColumn);

            MiicColumn labelIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishLabelRelation>(),
             Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelID));
            column.Add(labelIDColumn);

            MiicColumn labelNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishLabelRelation>(),
             Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelName));
            column.Add(labelNameColumn);

            MiicColumn microAccessoryIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                string.Empty,
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID),
                "CommunityAccessoryInfoID");
            column.Add(microAccessoryIDColumn);
            MiicColumn fileNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileName));
            column.Add(fileNameColumn);
            MiicColumn filePathColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FilePath));
            column.Add(filePathColumn);
            MiicColumn uploadTime = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, DateTime?>(o => o.UploadTime));
            column.Add(uploadTime);
            MiicColumn fileTypeColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileType));
            column.Add(fileTypeColumn);

            MiicColumn orgNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<Miic.Manage.User.SimplePersonUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.OrgName));
            column.Add(orgNameColumn);
            MiicColumn userUrlColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<Miic.Manage.User.SimplePersonUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.UserUrl));
            column.Add(userUrlColumn);
            MiicColumn userTypeColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<Miic.Manage.User.SimplePersonUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Manage.User.SimplePersonUserView, string>(o => o.UserType));
            column.Add(userTypeColumn);
            try
            {
                if (page == null)
                {
                    result = dbService.GetInformations(column, relation, condition, out message);
                }
                else
                {
                    result = dbService.GetInformationsPage(column, relation, condition, page, out message, MiicDBPageRowNumberSetting.DenseRank);
                }
            }
            catch (Exception ex)
            {
                LogicLog log = new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                };
                Config.IlogicLogService.Write(log);
            }
            return result;
        }

        /// <summary>
        /// 获取某人草稿总数
        /// </summary>
        /// <param name="keywordView">关键字视图</param>
        /// <returns>个人草稿总数</returns>
        public int GetDraftInfoCount(DraftSearchView keywordView)
        {
            Contract.Requires<ArgumentNullException>(keywordView != null, "参数keywordView:不能为空");
            int result = 0;
            string message = string.Empty;
            MiicConditionCollections condition = keywordView.visitor(this);
            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID));
            try
            {
                result = dbService.GetCount<PublishInfo>(column, condition, out message);
            }
            catch (Exception ex)
            {
                LogicLog log = new LogicLog()
                {
                    AppName = Config.AppName,
                    ClassName = ClassName,
                    NamespaceName = NamespaceName,
                    MethodName = MethodBase.GetCurrentMethod().Name,
                    Message = ex.Message,
                    Oper = Config.Oper
                };
                Config.IlogicLogService.Write(log);
            }
            return result;
        }

        /// <summary>
        /// 是否有行业圈子发布的内容
        /// </summary>
        /// <param name="communityID">行业圈子ID</param>
        /// <returns>Yes/No</returns>
        public bool HasCommunityPublish(string communityID)
        {

            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(communityID), "参数communityID:不能为空");
            bool result = false;

            string message = string.Empty;
            MiicCondition communityIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.CommunityID),
                communityID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            MiicConditionSingle condition = new MiicConditionSingle(communityIDCondition);
            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishLabelRelation>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.PublishID));
            int count = dbService.GetCount<PublishLabelRelation>(column, condition, out message, true);
            if (count > 0)
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// 获取用户对于某行业圈文章的行为状态
        /// </summary>
        /// <param name="behaviorView">用户行为视图</param>
        /// <returns>用户对于某行业圈文章的行为状态（是否点赞、是否点踩、是否举报、是否收藏）</returns>
        DataTable IPublishInfo.GetMyCommunityBehaviorFlags(MyCommunityBehaviorView behaviorView)
        {
            Contract.Requires<ArgumentNullException>(behaviorView != null, "参数behaviorView：不能为空！");
            DataTable result = new DataTable();
            string message = string.Empty;
            string sql = behaviorView.visitor(this);
            try
            {
                result = dbService.querySql(sql, out message);
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
