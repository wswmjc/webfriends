using Miic.Base;
using Miic.Base.Setting;
using Miic.DB;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Common;
using Miic.Friends.Common.Setting;
using Miic.Friends.Community.Setting;
using Miic.Friends.General.SimpleGroup;
using Miic.Friends.Notice.Setting;
using Miic.Friends.SimpleGroup;
using Miic.Log;
using Miic.Manage.User;
using Miic.Manage.User.Setting;
using Miic.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
namespace Miic.Friends.Community
{
    public partial class CommunityInfoDao : RelationCommon<CommunityInfo, CommunityMember>, ICommunityInfo
    {
        private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
        private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
        private static readonly string DefaultLogoUrl = WebConfigurationManager.AppSettings["DefaultCommunityLogoUrl"].ToString();
        private static readonly string CommunityLogoPath;
        private static readonly string CommunityLogoTempPath;
        private static readonly CommunityHotRateConfigSection communityHotRateConfigSection = (CommunityHotRateConfigSection)WebConfigurationManager.GetSection("CommunityHotRateConfigSection");
        static CommunityInfoDao() 
        {
            if (WebConfigurationManager.AppSettings["CommunityLogoPath"] == null)
            {
                CommunityLogoPath = Settings.Default.GroupLogoPath;
            }
            else 
            {
                CommunityLogoPath = WebConfigurationManager.AppSettings["CommunityLogoPath"].ToString();
            }
            CommunityLogoTempPath = CommunityLogoPath.Replace("/file", "/file/temp");
        }
        public CommunityInfoDao() { }
        bool ICommon<CommunityInfo>.Insert(CommunityInfo communityInfo)
        {
            Contract.Requires<ArgumentNullException>(communityInfo != null, "参数communityInfo：不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(communityInfo.ID), "参数communityInfo.ID：不能为空！");
            bool result = false;
            string message = string.Empty,
                message1 = string.Empty,
                message2 = string.Empty;
            List<string> sqls = new List<string>(); 
            communityInfo.Valid = ((int)MiicValidTypeSetting.Valid).ToString();
            communityInfo.MemberCount = 1;
            communityInfo.CanSearch = ((int)MiicYesNoSetting.Yes).ToString();
            sqls.Add(DBService.InsertSql<CommunityInfo>(communityInfo, out message1));

            //插入创建人
            sqls.Add(DBService.InsertSql<CommunityMember>(new CommunityMember()
            {
                ID = Guid.NewGuid().ToString(),
                CommunityID = communityInfo.ID,
                IsAdmin = ((int)MiicYesNoSetting.Yes).ToString(),
                JoinTime = DateTime.Now,
                MemberID = communityInfo.CreaterID,
                MemberName = communityInfo.CreaterName
            }, out message2));
            try
            {
                if (!string.IsNullOrEmpty(communityInfo.LogoUrl))
                {
                    bool fileResult = false;
                    try
                    {
                        string dest = HttpContext.Current.Server.MapPath(communityInfo.LogoUrl);
                        string source = HttpContext.Current.Server.MapPath(CommunityLogoTempPath + Path.GetFileName(dest));
                        File.Copy(source, dest, true);
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
                        result = dbService.excuteSqls(sqls, out message2);
                    }
                }
                else
                {
                    result = dbService.excuteSqls(sqls, out message2);
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
                InsertCache(communityInfo);
            }
            return result;
        }

        bool ICommon<CommunityInfo>.Update(CommunityInfo communityInfo)
        {
            Contract.Requires<ArgumentNullException>(communityInfo != null, "参数communityInfo:不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(communityInfo.ID), "参数communityInfo.ID:不能为空，因为是主键");
            int count = 0;
            string message = string.Empty;
            bool result = false;
            bool fileResult = false;
            try
            {
                CommunityInfo temp = ((ICommon<CommunityInfo>)this).GetInformation(communityInfo.ID);
                if (!string.IsNullOrEmpty(communityInfo.LogoUrl)
                   && temp.LogoUrl != communityInfo.LogoUrl)
                {
                    try
                    {
                            if (!string.IsNullOrEmpty(temp.LogoUrl))
                            {
                                File.Delete(HttpContext.Current.Server.MapPath(temp.LogoUrl));
                                File.Delete(HttpContext.Current.Server.MapPath(CommunityLogoTempPath + Path.GetFileName(temp.LogoUrl)));
                            }
                            string dest = HttpContext.Current.Server.MapPath(communityInfo.LogoUrl);
                            string source = HttpContext.Current.Server.MapPath(CommunityLogoTempPath + Path.GetFileName(dest));
                            File.Move(source, dest);
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

                if (fileResult == true)
                {
                    result = dbService.Update(communityInfo, out count, out message);
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
                DeleteCache(true, o => o.ID == communityInfo.ID, o => o.CommunityID == communityInfo.ID);
            }
            return result;
        }

        CommunityInfo ICommon<CommunityInfo>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            CommunityInfo result = null;
            string message = string.Empty;
            try
            {
                result = items.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new CommunityInfo
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
                    result = Config.Serializer.Deserialize<CommunityInfo>(serializer);
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
        /// 能否删除行业圈子
        /// </summary>
        /// <param name="id">行业圈子ID</param>
        /// <param name="communityInfo">行业圈子基本信息</param>
        /// <returns>Yes/No</returns>
        private bool CanDeleteCommunity(string id, out CommunityInfo communityInfo) 
        {
            bool result = false;
            List<CommunityMember> memberList = ((ICommunityInfo)this).GetMemberInfoListByCommunityID(id);
            communityInfo = ((ICommon<CommunityInfo>)this).GetInformation(id);
            IMessageInfo ImessageInfo = new MessageInfoDao();
            bool hasTopic =ImessageInfo.HasCommunityTopic(id);
            IPublishInfo IpublishInfo = new PublishInfoDao();
            bool hasPublish = IpublishInfo.HasCommunityPublish(id);
             if (memberList.Count == 1 
                && memberList[0].IsAdmin == ((int)MiicYesNoSetting.Yes).ToString() 
                &&communityInfo!=null
                && communityInfo.CreaterID == memberList[0].MemberID
                &&hasPublish==false
                &&hasTopic==false)
            {//删除圈子
                result = true;
             }
             return result;
        }
        bool ICommon<CommunityInfo>.Delete(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            bool result = false;
            string message1 = string.Empty,
                message2 = string.Empty,
                message3 = string.Empty,
                message4 = string.Empty,
                message5 = string.Empty,
                message6 = string.Empty,
                messagex = string.Empty;
            List<string> sqls = new List<string>();
            bool fileResult = false;
            CommunityInfo communityInfo=null;

            if (CanDeleteCommunity(id,out communityInfo)==true)
            {//删除圈子
                try
                {
                    if ((!string.IsNullOrEmpty(communityInfo.LogoUrl)) && communityInfo.LogoUrl != DefaultLogoUrl)
                    {
                        File.Delete(HttpContext.Current.Server.MapPath(communityInfo.LogoUrl));
                        File.Delete(HttpContext.Current.Server.MapPath(CommunityLogoTempPath + Path.GetFileName(communityInfo.LogoUrl)));
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
                    //删除圈子
                    sqls.Add(DBService.DeleteSql(new CommunityInfo()
                    {
                        ID = id
                    }, out message1));
                    //删除圈子成员
                    sqls.Add(DBService.DeleteConditionSql<CommunityMember>(new MiicConditionSingle(
                        new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.CommunityID),
                                            id,
                                            DbType.String,
                                            MiicDBOperatorSetting.Equal)), out message2));
                    //删除@提醒 Topic
                    sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Community).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.PublishInfo).ToString() + "' and PUBLISH_ID in (select ID from COMMUNITY_TOPIC_INFO where COMMUNITY_ID='" + id + "') ");
                    sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Community).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.PublishInfo).ToString() + "' and PUBLISH_ID in (select ID from COMMUNITY_PUBLISH_LABEL_RELATION where COMMUNITY_ID='" + id + "') ");
                    //删除@提醒 Message
                    sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Community).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.Message).ToString() + "' and PUBLISH_ID in (select ID from COMMUNITY_MESSAGE_INFO where TOPIC_ID=(select ID from COMMUNITY_TOPIC_INFO where COMMUNITY_ID='" + id + "'))");
                    sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Community).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.Message).ToString() + "' and PUBLISH_ID in (select ID from COMMUNITY_COMMENT_INFO where PUBLISH_ID in(select ID from COMMUNITY_PUBLISH_LABEL_RELATION where COMMUNITY_ID='" + id + "'))");
                    try
                    {
                        result = dbService.excuteSqls(sqls, out messagex);
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
            else
            {//设置圈子失效
                //设置圈子表失效
                sqls.Add(DBService.UpdateSql<CommunityInfo>(new CommunityInfo()
                {
                    ID = id,
                    Valid = ((int)MiicValidTypeSetting.InValid).ToString(),
                    EndTime = DateTime.Now
                }, out message3));
                //设置圈子信息级联失效
                MiicCondition publishLabelRelationIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.CommunityID),
                  id,
                  DbType.String,
                  MiicDBOperatorSetting.Equal);
                sqls.Add(DBService.UpdateConditionSql<PublishLabelRelation>(new PublishLabelRelation()
                {
                    Valid = ((int)MiicValidTypeSetting.InValid).ToString()
                }, new MiicConditionSingle(publishLabelRelationIDCondition), out message4));
                //设置标签失效
                MiicCondition labelIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.CommunityID),
                    id,
                    DbType.String,
                    MiicDBOperatorSetting.Equal);
                sqls.Add(DBService.UpdateConditionSql<LabelInfo>(new LabelInfo()
                {
                    Valid = ((int)MiicValidTypeSetting.InValid).ToString(),
                    EndTime = DateTime.Now
                }, new MiicConditionSingle(labelIDCondition), out message5));
                //设置圈子话题级联失效
                MiicCondition topicIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<TopicInfo, string>(o => o.CommunityID),
                     id,
                     DbType.String,
                     MiicDBOperatorSetting.Equal);
                sqls.Add(DBService.UpdateConditionSql<TopicInfo>(new TopicInfo()
                {
                    Valid = ((int)MiicValidTypeSetting.InValid).ToString(),
                    EndTime = DateTime.Now
                }, new MiicConditionSingle(topicIDCondition), out message6));
                //删除@提醒 Topic
                sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Community).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.PublishInfo).ToString() + "' and PUBLISH_ID in (select ID from COMMUNITY_TOPIC_INFO where COMMUNITY_ID='" + id + "') ");
                sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Community).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.PublishInfo).ToString() + "' and PUBLISH_ID in (select ID from COMMUNITY_PUBLISH_LABEL_RELATION where COMMUNITY_ID='" + id + "') ");
                //删除@提醒 Message
                sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Community).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.Message).ToString() + "' and PUBLISH_ID in (select ID from COMMUNITY_MESSAGE_INFO where TOPIC_ID=(select ID from COMMUNITY_TOPIC_INFO where COMMUNITY_ID='" + id + "'))");
                sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Community).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.Message).ToString() + "' and PUBLISH_ID in (select ID from COMMUNITY_COMMENT_INFO where PUBLISH_ID in(select ID from COMMUNITY_PUBLISH_LABEL_RELATION where COMMUNITY_ID='" + id + "'))");
                try
                {
                    result = dbService.excuteSqls(sqls, out messagex);
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
            if (result == true)
            {
                DeleteCache(true, o => o.ID == id, o => o.CommunityID == id);
                LabelInfoDao.DeleteCacheByCommunityID(id);
                MessageInfoDao.DeleteCacheByCommunityID(id);
            }
            return result;
        }
        /// <summary>
        /// 删除行业圈子
        /// </summary>
        /// <param name="id">行业圈子id</param>
        /// <returns>Yes/No</returns>
        /// <remarks>级联成员、发布内容、标签主题等</remarks>
        bool ICommunityInfo.DeepDelete(string id)
        {
            bool result = false;
            string message = string.Empty,
                message1 = string.Empty,
                message2 = string.Empty,
                message3 = string.Empty,
                message4 = string.Empty,
                message5 = string.Empty,
                message6 = string.Empty ;

            List<string> sqls = new List<string>();
            //1删除行业圈子
            sqls.Add(DBService.DeleteSql(new CommunityInfo() { ID = id }, out message1));
            //2删除圈子成员
            MiicCondition communityIDMemberCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.CommunityID),
                id,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            MiicConditionSingle member = new MiicConditionSingle(communityIDMemberCondition);
            //3.1删除讨论内容
            sqls.Add(DBService.DeleteConditionSql<CommunityMember>(member, out message2));
            MiicCondition communityIDTopicCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<TopicInfo, string>(o => o.CommunityID),
                id,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            MiicConditionSingle topic = new MiicConditionSingle(communityIDTopicCondition);
            sqls.Add(DBService.DeleteConditionSql<MessageInfo>(topic, out message3));
            //3.2删除讨论
            sqls.Add(DBService.DeleteSql(new TopicInfo() { ID = id }, out message4));
            //4.删除话题
            //4.1删除标签
            MiicCondition communityIDLabelCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.CommunityID),
              id,
              DbType.String,
              MiicDBOperatorSetting.Equal);
            MiicConditionSingle label = new MiicConditionSingle(communityIDLabelCondition);
            sqls.Add(DBService.DeleteConditionSql<LabelInfo>(label, out message5));
            //4.2删除关系
            MiicCondition communityIDRelationCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.CommunityID),
             id,
             DbType.String,
             MiicDBOperatorSetting.Equal);
            MiicConditionSingle publishLabelRelation = new MiicConditionSingle(communityIDRelationCondition);
            sqls.Add(DBService.DeleteConditionSql<PublishLabelRelation>(publishLabelRelation, out message6));
            //5删除话题行为
            //5.1赞
            sqls.Add("delete from COMMUNITY_PRAISE_INFO where PUBLISH_ID in(select PUBLISH_ID from COMMUNITY_PUBLISH_LABEL_RELATION where COMMUNITY_ID='" + id + "')");
            //5.2踩
            sqls.Add("delete from COMMUNITY_TRADE_INFO where PUBLISH_ID in(select PUBLISH_ID from COMMUNITY_PUBLISH_LABEL_RELATION where COMMUNITY_ID='" + id + "')");
            //5.3浏览
            sqls.Add("delete from COMMUNITY_BROWSE_INFO where PUBLISH_ID in(select PUBLISH_ID from COMMUNITY_PUBLISH_LABEL_RELATION where COMMUNITY_ID='" + id + "')");
            //5.4评论
            sqls.Add("delete from COMMUNITY_COMMENT_INFO where PUBLISH_ID in(select PUBLISH_ID from COMMUNITY_PUBLISH_LABEL_RELATION where COMMUNITY_ID='" + id + "')");
            bool resultFileDelete = false;
            try
            {
                CommunityInfo temp = ((ICommon<CommunityInfo>)this).GetInformation(id);
                if (temp != null)
                {
                    File.Delete(HttpContext.Current.Server.MapPath(temp.LogoUrl));
                    File.Delete(HttpContext.Current.Server.MapPath(CommunityLogoTempPath + Path.GetFileName(temp.LogoUrl)));
                }
                List<AccessoryInfo> file = (new PublishInfoDao()).GetAccessoryListByCommunityID(id);
                foreach (var item in file.AsEnumerable())
                {
                    File.Delete(HttpContext.Current.Server.MapPath(item.FilePath));
                    if (item.FileType == ((int)AccFileTypeSetting.Photo).ToString())
                    {
                        File.Delete(HttpContext.Current.Server.MapPath("/file/temp/Community/Photo/" + Path.GetFileName(item.FilePath)));
                    }
                    else
                    {
                        File.Delete(HttpContext.Current.Server.MapPath("/file/temp/Community/File/" + Path.GetFileName(item.FilePath)));
                    }
                }
                resultFileDelete = true;
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
            if (resultFileDelete == true)
            {
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
                    DeleteCache(true, o => o.ID == id, o => o.CommunityID == id);
                    MessageInfoDao.DeleteCacheByCommunityID(id);
                    LabelInfoDao.DeleteCacheByCommunityID(id);
                }
            }

            return result;
        }

        /// <summary>
        /// 搜索我的/某人的圈子信息列表
        /// </summary>
        /// <param name="searchView">搜索视图</param>
        /// <param name="page">分页，默认不分页</param>
        /// <returns>圈子信息列表</returns>
        public DataTable Search(GeneralSimpleGroupSearchView searchView, MiicPage page)
        {
            DataTable result = new DataTable();
            string message = string.Empty;
            MiicConditionCollections condition = searchView.visitor(this);
            List<MiicOrderBy> order = new List<MiicOrderBy>();
            order.Add(new MiicOrderBy()
            {
                Desc = false,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.Name)
            });
            order.Add(new MiicOrderBy()
            {
                Desc = true,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, int?>(o => o.MemberCount)
            });
            condition.order = order;
            MiicColumnCollections column = new MiicColumnCollections();
            MiicColumn searchColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CommunitySearchInfo>());
            column.Add(searchColumn);
            try
            {
                if (page == null)
                {
                    result = dbService.GetInformations<CommunitySearchInfo>(column, condition, out message);
                }
                else
                {
                    result = dbService.GetInformationsPage<CommunitySearchInfo>(column, condition, page, out message);
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
        /// 搜索我的/某人的圈子数
        /// </summary>
        /// <param name="searchView">搜索视图</param>
        /// <returns>圈子数</returns>
        public int GetSearchCount(GeneralSimpleGroupSearchView searchView)
        {
            string message = string.Empty;
            int result = 0;
            MiicConditionCollections condition = searchView.visitor(this);
            MiicColumn idColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CommunitySearchInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CommunitySearchInfo, string>(o => o.ID));
            try
            {
                result = dbService.GetCount<CommunitySearchInfo>(idColumn, condition, out message);
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
        /// 获取某人的行业圈子列表(主页右侧显示)
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="top">top 默认8</param>
        /// <returns>行业圈子列表</returns>
        public DataTable GetCommunityInfoList(string userID, int top)
        {
            DataTable result = new DataTable();
            string message = string.Empty;
            MiicConditionCollections conditions = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition memberIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.MemberID),
                userID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, memberIDCondition));
            MiicCondition validCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.Valid),
              ((int)MiicValidTypeSetting.Valid).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(validCondition));
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<CommunityMember>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.CommunityID),
                Config.Attribute.GetSqlTableNameByClassName<CommunityInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.ID),
                 MiicDBOperatorSetting.Equal,
                 MiicDBRelationSetting.InnerJoin);
            MiicColumnCollections column = new MiicColumnCollections(new MiicTop(top));
            MiicColumn idColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CommunityInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.ID));
            column.Add(idColumn);
            MiicColumn nameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CommunityInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.Name));
            column.Add(nameColumn);
            MiicColumn urlColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CommunityInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.LogoUrl));
            column.Add(urlColumn);
            try
            {
                result = dbService.GetInformations(column, relation, conditions, out message);
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

        public bool HaveCommunity(string userID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(userID), "参数userID:不能为空");
            bool result = false;
            int count;
            string message = string.Empty;
            MiicConditionCollections conditions = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition userIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.MemberID),
                userID,
                DbType.String,
               MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, userIDCondition));
            MiicCondition validCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.Valid),
              ((int)MiicValidTypeSetting.Valid).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(validCondition));
            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CommunityInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.ID)); 
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<CommunityInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.ID),
                Config.Attribute.GetSqlTableNameByClassName<CommunityMember>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.CommunityID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.InnerJoin);

            try
            {
                count = dbService.GetCount(column, relation, conditions, out message);
                if (count != 0 && count != null)
                {
                    result = true;
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
        /// 搜索热门圈子信息列表
        /// </summary>
        /// <param name="keywordView">关键字视图</param>
        /// <param name="page">分页</param>
        /// <returns>热门圈子信息列表</returns>
        public DataTable SearchHotCommunity(MyKeywordView keywordView, MiicPage page)
        {
            DataTable result = new DataTable();
            string message = string.Empty;
            string querySql = GetHotCommunityViewSql(0, keywordView, page);
            try
            {
                result = dbService.querySql(querySql, out message);
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
        /// 获取搜索热门圈子信息列表数
        /// </summary>
        /// <param name="keywordView">关键字视图</param>
        /// <returns>热门圈子信息列表数</returns>
        public int GetSearchHotCommunityCount(Miic.Friends.Common.NoPersonKeywordView keywordView)
        {
            int result = 0;
            string message = string.Empty;
            string querySql = GetHotCommunityViewCountSql(keywordView);
            try
            {
                result = dbService.GetSqlCount(querySql, out message);
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
        /// 获取热门圈子列表(主页右侧显示)
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="top">top 默认8</param>
        /// <returns>热门圈子列表</returns>
        public DataTable GetHotCommunityRecommendInfoList(int top)
        {
            DataTable result = new DataTable();
            string message = string.Empty;
            string querySql = GetHotCommunityViewSql(top);
            try
            {
                result = dbService.querySql(querySql, out message);
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
        /// 新增行业圈子
        /// </summary>
        /// <param name="communityInfo">行业圈子信息</param>
        /// <param name="memberApplications">成员邀请列表</param>
        /// <returns>Yes/No</returns>
        public bool Insert(CommunityInfo communityInfo, List<CommunityApplicationInfo> memberApplications)
        {

            Contract.Requires<ArgumentNullException>(communityInfo != null, "参数communityInfo：不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(communityInfo.ID), "参数communityInfo.ID：不能为空！");
            Contract.Requires<ArgumentNullException>(memberApplications != null && memberApplications.Count != 0, "参数cmemberApplications：不能为空！");
            bool result = false;
            string message = string.Empty,
                message1 = string.Empty,
                message2 = string.Empty,
                message3 = string.Empty;
            List<string> sqls = new List<string>();

            //系统初始化设置
            communityInfo.Valid = ((int)MiicValidTypeSetting.Valid).ToString();
            communityInfo.CanSearch = ((int)MiicYesNoSetting.Yes).ToString();
            communityInfo.MemberCount = 1;

            //插入圈子
            sqls.Add(DBService.InsertSql<CommunityInfo>(communityInfo, out message1));

            //插入创建人
            sqls.Add(DBService.InsertSql<CommunityMember>(new CommunityMember()
            {
                ID = Guid.NewGuid().ToString(),
                CommunityID = communityInfo.ID,
                IsAdmin = ((int)MiicYesNoSetting.Yes).ToString(),
                JoinTime = DateTime.Now,
                MemberID = communityInfo.CreaterID,
                MemberName = communityInfo.CreaterName
            }, out message2));

            //插入成员
            foreach (var item in memberApplications.AsEnumerable())
            {
                sqls.Add(DBService.InsertSql<CommunityApplicationInfo>(new CommunityApplicationInfo()
                {
                    ID = Guid.NewGuid().ToString(),
                    CommunityID = communityInfo.ID,
                    ApplicationTime = DateTime.Now,
                    ResponseStatus = ((int)ApplyStatusSetting.Invite).ToString(),
                    Remark = communityInfo.CreaterName + "邀请您加入" + communityInfo.Name + "行业圈子。",
                    MemberID = item.MemberID,
                    MemberName = item.MemberName
                }, out message3));
            }

            try
            {
                if (communityInfo.LogoUrl != DefaultLogoUrl)
                {
                    bool fileResult = false;
                    try
                    {
                        string dest = HttpContext.Current.Server.MapPath(communityInfo.LogoUrl);
                        string source = HttpContext.Current.Server.MapPath(CommunityLogoTempPath + Path.GetFileName(dest));
                        File.Copy(source, dest, true);
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
                        result = dbService.excuteSqls(sqls, out message2);
                    }
                }
                else
                {
                    result = dbService.excuteSqls(sqls, out message2);
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
                InsertCache(communityInfo);
            }
            return result;
        }

        /// <summary>
        /// 动态获取热门圈子视图
        /// </summary>
        /// <param name="top">是否获取前几条记录 默认为0 获取全部</param>
        /// <param name="view">是否具有条件视图（关键字） 默认为null 附加条件</param>
        /// <param name="page">是否需要分页 默认为null 不需要分页</param>
        /// <returns>查询语句</returns>
        private static string GetHotCommunityViewSql(int top = 0, MyKeywordView view = null, MiicPage page = null)
        {
            //成员数量系数
            string memberRatio = communityHotRateConfigSection.MemberRatio.ToString(); 
            //话题数量系数
            string topicRatio = communityHotRateConfigSection.TopicRatio.ToString();
            //话题讨论数量系数
            string topicMessageRatio = communityHotRateConfigSection.TopicMessageRatio.ToString();
            //圈子信息数量系数
            string publishRatio = communityHotRateConfigSection.PublishRatio.ToString(); 
            //圈子信息行为操作数量系数
            string publishPartakeRatio = communityHotRateConfigSection.PublishPartakeRatio.ToString(); 

            string result = string.Empty;
            if (page == null)
            {
                result = "select" + (top == 0 ? string.Empty : " TOP " + top) + " *";
                result += " from ";
                result += " MIIC_FRIENDS.dbo.GetCommunityPartakeInfo('" + memberRatio + "','" + topicRatio + "','" + topicMessageRatio + "','" + publishRatio + "','" + publishPartakeRatio + "') ";
                result += " WHERE " + (view == null ? "1=1" : Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.Name) + " LIKE '%" + view.Keyword + "%'");
                result += " ORDER BY PARTAKE_NUM DESC";
            }
            else
            {
                result = "WITH INFO_PAGE AS (";
                result += "SELECT ROW_NUMBER() OVER ( ORDER BY TEMP.PARTAKE_NUM DESC) AS ROW ,TEMP.* FROM (";
                result += " select COMMUNITY_TEMP.*,JOIN_TIME from";
                result += " (SELECT * FROM ";
                result += " MIIC_FRIENDS.DBO.GetCommunityPartakeInfo('" + memberRatio + "','" + topicRatio + "','" + topicMessageRatio + "','" + publishRatio + "','" + publishPartakeRatio + "')";
                result += " WHERE " + (view == null ? "1=1" : Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.Name) + " LIKE '%" + view.Keyword + "%'");
                result += ") as COMMUNITY_TEMP left join ";
                result += " (select * from COMMUNITY_MEMBER ";
                result += " where COMMUNITY_MEMBER.MEMBER_ID='" + view.UserID + "') as COMMUNITY_MEMBER_TEMP";
                result += " on COMMUNITY_TEMP.ID=COMMUNITY_MEMBER_TEMP.COMMUNITY_ID ";
                result += " ) AS TEMP";
                result += ")";
                result += "SELECT * FROM INFO_PAGE WHERE ROW BETWEEN " + page.pageStart + " AND " + page.pageEnd;
            }
            return result;
        }

        private static string GetHotCommunityViewCountSql(Miic.Friends.Common.NoPersonKeywordView view = null, MiicPage page = null)
        {
            //成员数量系数
            string memberRatio = communityHotRateConfigSection.MemberRatio.ToString();
            //话题数量系数
            string topicRatio = communityHotRateConfigSection.TopicRatio.ToString();
            //话题讨论数量系数
            string topicMessageRatio = communityHotRateConfigSection.TopicMessageRatio.ToString();
            //圈子信息数量系数
            string publishRatio = communityHotRateConfigSection.PublishRatio.ToString();
            //圈子信息行为操作数量系数
            string publishPartakeRatio = communityHotRateConfigSection.PublishPartakeRatio.ToString();

            string result = string.Empty;
            if (page == null)
            {
                result = "SELECT count(*) FROM "
                            + " MIIC_FRIENDS.DBO.GetCommunityPartakeInfo('" + memberRatio + "','" + topicRatio + "','" + topicMessageRatio + "','" + publishRatio + "','" + publishPartakeRatio + "')"
                            + " WHERE " + (view == null ? "1=1" : Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.Name) + " LIKE '%" + view.Keyword + "%'");
            }
            return result;
        }




        public bool Update(CommunityInfo communityInfo, List<CommunityMember> members)
        {
            bool result = false;
            bool fileResult = false;
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            List<string> sqls = new List<string>();
            CommunityInfo temp = ((ICommon<CommunityInfo>)this).GetInformation(communityInfo.ID);
            sqls.Add(DBService.UpdateSql<CommunityInfo>(communityInfo, out message1));
            foreach (CommunityMember item in members.AsEnumerable())
            {
                sqls.Add(DBService.UpdateSql(item, out message2));
            }
            if (!string.IsNullOrEmpty(communityInfo.LogoUrl)
                  &&temp!=null
                  && temp.LogoUrl != communityInfo.LogoUrl)
            {
                try
                {
                    if (!string.IsNullOrEmpty(temp.LogoUrl))
                    {
                        File.Delete(HttpContext.Current.Server.MapPath(temp.LogoUrl));
                        File.Delete(HttpContext.Current.Server.MapPath(CommunityLogoTempPath + Path.GetFileName(temp.LogoUrl)));
                    }
                    string dest = HttpContext.Current.Server.MapPath(communityInfo.LogoUrl);
                    string source = HttpContext.Current.Server.MapPath(CommunityLogoTempPath + Path.GetFileName(dest));
                    File.Copy(source, dest, true);
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
            if (result == true)
            {
                DeleteCache(true, o => o.ID == communityInfo.ID, o => o.CommunityID == communityInfo.ID);
            }
            return result;
        }



        bool ICommunityInfo.PersonQuit(string communityID, string userID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(communityID), "参数communityID：不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(userID), "参数userID：不能为空！");
            bool result = false;
            string message = string.Empty;
            string message1 = string.Empty;
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition communityIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.CommunityID),
                communityID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, communityIDCondition));
            MiicCondition userIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.MemberID),
                userID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(userIDCondition));
            lock (syncRoot)
            {
                CommunityInfo communityInfo = ((ICommon<CommunityInfo>)this).GetInformation(communityID);
                List<string> sqls = new List<string>();
                sqls.Add(DBService.DeleteConditionsSql<CommunityMember>(condition, out message1));
                sqls.Add(DBService.UpdateSql<CommunityInfo>(new CommunityInfo()
                {
                    ID = communityID,
                    MemberCount = communityInfo.MemberCount - 1
                }, out message1));
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
                    lock (syncRoot)
                    {
                        DeleteCache(o => o.CommunityID == communityID && o.MemberID == userID);
                        if (items.Find(o => o.ID == communityID) != null)
                        {
                            items.Find(o => o.ID == communityID).MemberCount = communityInfo.MemberCount - 1;
                        }
                    }
                }
            }
            return result;
        }



        bool ICommunityInfo.IsCreater(string userID, string communityID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(userID), "参数userID:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(communityID), "参数communityID:不能为空");
            bool result = false;
            string message = string.Empty;
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition createrCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.CreaterID),
               userID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, createrCondition));
            MiicCondition communityIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.ID),
                       communityID,
                       DbType.String,
                       MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(communityIDCondition));
            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CommunityInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.ID));
            try
            {
                int count = dbService.GetCount<CommunityInfo>(column, condition, out message);
                if (count > 0)
                {
                    result = true;
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


        public DataTable PersonTrendsSearch(GeneralSimpleGroupSearchView searchView, MiicPage page)
        {
            Contract.Requires<ArgumentNullException>(searchView != null, "参数searchView:不能为空");
            DataTable result = new DataTable();
            string message = string.Empty;
            string userID = string.Empty;
            if (searchView is MySimpleGroupSearchView)
            {
                userID = ((MySimpleGroupSearchView)searchView).UserID;
            }
            else
            {
                userID = ((PersonSimpleGroupSearchView)searchView).UserID;
            }
            try
            {
                if (page != null)
                {
                   
                    result = dbService.querySql("select * from SearchTrendsCommunityInfosPage('"+userID+"','"+searchView.Keyword+"',"+page.pageStart+","+page.pageEnd+")", out message);
                }
                else 
                {
                    result = dbService.querySql("select * from SearchTrendsCommunityInfos('"+userID+"','"+searchView.Keyword+"')", out message);
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

        public int GetPersonTrendsSearchCount(GeneralSimpleGroupSearchView searchView)
        {
            Contract.Requires<ArgumentNullException>(searchView != null, "参数searchView:不能为空");
            int result = 0;
            string message = string.Empty;
            string userID = string.Empty;
            if (searchView is MySimpleGroupSearchView)
            {
                userID = ((MySimpleGroupSearchView)searchView).UserID;
            }
            else
            {
                userID = ((PersonSimpleGroupSearchView)searchView).UserID;
            }
            try
            {
                DataTable dt = dbService.querySql("select * from GetSearchTrendsCommunityInfosCount('"+userID+"','"+searchView.Keyword+"')", out message);
                if (dt.Rows.Count > 0)
                {
                    result = int.Parse(dt.Rows[0][0].ToString());
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

        DataTable ICommunityInfo.GetPublishInfosByLabelID(string labelID, MiicPage page)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(labelID), "参数labelID:不能为空");
            string message = string.Empty;
            DataTable result = new DataTable();
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition labelIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelID),
                labelID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, labelIDCondition));
            MiicCondition validCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.Valid),
                ((int)MiicValidTypeSetting.Valid).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(validCondition));
            MiicRelationCollections relation = new MiicRelationCollections(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>());
            MiicFriendRelation labelPublishRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID),
                Config.Attribute.GetSqlTableNameByClassName<PublishLabelRelation>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.PublishID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.LeftJoin);
            relation.Add(labelPublishRelation);
            MiicFriendRelation publishAccRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<PublishInfo, string>(o => o.ID),
                Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.PublishID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.LeftJoin);
            relation.Add(publishAccRelation);
            MiicColumnCollections columns = new MiicColumnCollections();
            MiicColumn publishColumns = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishInfo>());
            columns.Add(publishColumns);
            MiicColumn fileIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),"",
              Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.ID),"FILE_ID");
            columns.Add(fileIDColumn);
            MiicColumn fileNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileName));
            columns.Add(fileNameColumn);
            MiicColumn filePathColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FilePath));
            columns.Add(filePathColumn);
            MiicColumn fileTypeColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AccessoryInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AccessoryInfo, string>(o => o.FileType));
            columns.Add(fileTypeColumn);
            try
            {
                if (page != null)
                {
                    result = dbService.GetInformationsPage(columns, relation, condition, page, out message, MiicDBPageRowNumberSetting.DenseRank);
                }
                else 
                {
                    result = dbService.GetInformations(columns, relation, condition, out message);
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

        int ICommunityInfo.GetPublishInfosCountByLabelID(string labelID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(labelID), "参数labelID:不能为空");
            int result = 0;
            string message = string.Empty;
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition labelIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelID),
                labelID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, labelIDCondition));
            MiicCondition validCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.Valid),
                ((int)MiicValidTypeSetting.Valid).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(validCondition));
            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishLabelRelation>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.PublishID));
            try
            {
                result = dbService.GetCount<PublishLabelRelation>(column, condition, out message);
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

       

        bool ICommunityInfo.CanCreateCommunity(string userID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(userID), "参数userID:不能为空");
            bool result = false;
            IMiicSocialUser IsocialUser = new MiicSocialUserDao(false);
            MiicSocialUserInfo socialUser = IsocialUser.GetInformation(userID);
            if (socialUser.UserType == ((int)UserTypeSetting.Administrator).ToString()
                || socialUser.UserType == ((int)UserTypeSetting.LocalAdmin).ToString()
                || socialUser.UserType == ((int)UserTypeSetting.CountryAdmin).ToString())
            {
                result = true;
            }
            else
            {
                UserLevelSetting level = (UserLevelSetting)Enum.Parse(typeof(UserLevelSetting), WebConfigurationManager.AppSettings["CreateCommunityLevel"].ToString());
                if (int.Parse(socialUser.Level) >= (int)level)
                {
                    result = true;
                }
            }
            return result;
        }

        public DataTable GetCommunityToPersonStatistics(StatisticsSearchView searchView)
        {
            Contract.Requires<ArgumentNullException>(searchView != null, "参数searchView:不能为空");
            string message = string.Empty;
            string sql = "SELECT * FROM GetCommunityStatisticsWithTime('" + searchView.BeginTime + "','" + (string.IsNullOrEmpty(searchView.EndTime) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : searchView.EndTime) + "')";
            DataTable result = new DataTable();
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

        public DataTable GetCommunityToLabelStatistics(StatisticsSearchView searchView)
        {
            Contract.Requires<ArgumentNullException>(searchView != null, "参数searchView:不能为空");
            string message = string.Empty;
            string sql = "SELECT * FROM GetCommunityToLabelStatistics('" + searchView.CommunityID + "')";
            DataTable result = new DataTable();
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

        public DataTable GetAllCommunityWithLabelList()
        {
            string message = string.Empty;
            DataTable result = new DataTable();
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<CommunityInfo>(),
                 Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.ID),
                 Config.Attribute.GetSqlTableNameByClassName<LabelInfo>(),
                 Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.CommunityID),
                  MiicDBOperatorSetting.Equal,
                   MiicDBRelationSetting.LeftJoin);

            MiicColumnCollections column = new MiicColumnCollections();
            MiicColumn communityIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.CommunityID));
            column.Add(communityIDColumn);
            MiicColumn communityNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CommunityInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<CommunityInfo, string>(o => o.Name));
            column.Add(communityNameColumn);
            MiicColumn labelIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.ID));
            column.Add(labelIDColumn);
            MiicColumn labelNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.LabelName));
            column.Add(labelNameColumn);
            
            try
            {
                result = dbService.GetInformations(column, relation, new MiicConditionSingle(new MiicCondition("1","1", DbType.String, MiicDBOperatorSetting.Equal)), out message);
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
