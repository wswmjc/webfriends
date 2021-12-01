using Miic.Base;
using Miic.DB;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.General.SimpleGroup;
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
using System.Transactions;
using Miic.Draw;
using Miic.Manage.User;
using Miic.Base.Setting;
using System.Web.Configuration;
using Miic.Friends.SimpleGroup;
using Miic.Friends.Common.Setting;
using Miic.Friends.Notice.Setting;
using Miic.Properties;
namespace Miic.Friends.Group
{
    public partial class GroupInfoDao : RelationCommon<GroupInfo, GroupMember>, IGroupInfo
    {
        private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
        private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
        private static readonly string ManageUrl = WebConfigurationManager.AppSettings["ManageUrl"].ToString();
        //讨论组Logo路径
        private static readonly string GroupLogoPath;
        static GroupInfoDao() 
        {
            if (WebConfigurationManager.AppSettings["GroupLogoPath"] == null)
            {
                GroupLogoPath = Settings.Default.GroupLogoPath;
            }
            else 
            {
                GroupLogoPath = WebConfigurationManager.AppSettings["GroupLogoPath"].ToString();
            }
        }
        bool ICommon<GroupInfo>.Insert(GroupInfo groupInfo)
        {
            Contract.Requires<ArgumentNullException>(groupInfo != null, "参数groupInfo:不能为空");
            bool result = false;
            string message = string.Empty;
            string message1 = string.Empty;
            List<string> sqls = new List<string>();
            IMiicSocialUser ImiicSocialUser = new MiicSocialUserDao(false);
            MiicSocialUserInfo userTemp = ImiicSocialUser.GetInformation(groupInfo.CreaterID);
            sqls.Add(DBService.InsertSql(groupInfo, out message1));
            DrawService drawService = new DrawService();
            drawService.SaveFile = GroupLogoPath;
            List<string> url = new List<string>();
            url.Add(ManageUrl + userTemp.MicroUserUrl);
            groupInfo.LogoUrl = drawService.CreateImageByDiscussionGroupTypeByUrl(url);
            GroupMember member = new GroupMember()
            {
                ID = Guid.NewGuid().ToString(),
                GroupID = groupInfo.ID,
                JoinTime = DateTime.Now,
                MemberID = groupInfo.CreaterID,
                MemberName = groupInfo.CreaterName
            };
            sqls.Add(DBService.InsertSql(member, out message1));
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
                List<GroupMember> cache = new List<GroupMember>();
                cache.Add(member);
                InsertCaches(cache);
                InsertCache(groupInfo, cache);
            }
            return result;
        }

        bool ICommon<GroupInfo>.Update(GroupInfo groupInfo)
        {
            Contract.Requires<ArgumentNullException>(groupInfo != null, "参数groupInfo:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            try
            {
                result = dbService.Update(groupInfo, out count, out message);
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
                DeleteCache(false, o => o.ID == groupInfo.ID);
            }
            return result;
        }

        bool ICommon<GroupInfo>.Delete(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            bool result = false;
            string message = string.Empty,
                   message1 = string.Empty,
                   message2 = string.Empty,
                   message3 = string.Empty,
                   message4 = string.Empty;
            List<string> sqls = new List<string>();
            GroupInfo groupInfo=null;
            if (CanDeleteGroup(id,out groupInfo)==true)
            {
                bool fileResult = false;
                if (groupInfo != null)
                {
                    //1删除Logo
                    if (!string.IsNullOrEmpty(groupInfo.LogoUrl))
                    {
                        try
                        {
                            File.Delete(HttpContext.Current.Server.MapPath(GroupLogoPath+ Path.GetFileName(groupInfo.LogoUrl)));
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
                }
                if (fileResult == true)
                {
                    //删除小组
                    sqls.Add(DBService.DeleteSql(new GroupInfo() { ID = id }, out message1));
                    //删除小组成员
                    MiicCondition groupIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.GroupID),
                       id,
                       DbType.String,
                       MiicDBOperatorSetting.Equal);
                    MiicConditionSingle condition = new MiicConditionSingle(groupIDCondition);
                    sqls.Add(DBService.DeleteConditionSql<GroupMember>(condition, out message2));
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
                }
            }
            else
            {
                //设置小组失效
                //设置讨论组失效
                sqls.Add(DBService.UpdateSql<GroupInfo>(new GroupInfo()
                {
                    ID = id,
                    Valid = ((int)MiicValidTypeSetting.InValid).ToString(),
                    EndTime = DateTime.Now
                }, out message3));
                //设置讨论组话题级联失效
                MiicCondition topicIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<TopicInfo, string>(o => o.GroupID),
                    id,
                    DbType.String,
                    MiicDBOperatorSetting.Equal);
                sqls.Add(DBService.UpdateConditionSql<TopicInfo>(new TopicInfo()
                {
                    Valid = ((int)MiicValidTypeSetting.InValid).ToString(),
                    EndTime = DateTime.Now
                }, new MiicConditionSingle(topicIDCondition), out message4));
                //删除@提醒 Topic
                sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Group).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.PublishInfo).ToString() + "' and PUBLISH_ID in (select ID from GROUP_TOPIC_INFO where GROUP_ID='" + id + "') ");
                //删除@提醒 Message
                sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Group).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.Message).ToString() + "' and PUBLISH_ID in (select ID from GROUP_MESSAGE_INFO where TOPIC_ID=(select ID from GROUP_TOPIC_INFO where GROUP_ID='" + id + "'))");
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

            }

            if (result == true)
            {
                DeleteCache(true, o => o.ID == id, o => o.GroupID == id);
                MessageInfoDao.DeleteCacheByGroupID(id);
            }
            return result;
        }
        /// <summary>
        /// 是否能够删除讨论组
        /// </summary>
        /// <param name="id">GroupID</param>
        /// <param name="groupInfo">讨论组基本信息</param>
        /// <returns>Yes/No</returns>
        private bool CanDeleteGroup(string id,out GroupInfo groupInfo) 
        {
            bool result = false;
            List<GroupMember> memberList = ((IGroupInfo)this).GetGroupMemberList(id);
            groupInfo = ((ICommon<GroupInfo>)this).GetInformation(id);
            IMessageInfo ImessageInfo = new MessageInfoDao();
            bool hasGroupTopic= ImessageInfo.HasGroupTopic(id);
            if (memberList.Count == 1 && groupInfo.CreaterID == memberList[0].MemberID&&hasGroupTopic==false) 
            {
                result = true;
            }
            return result;
        }
        GroupInfo ICommon<GroupInfo>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            GroupInfo result = null;
            string message = string.Empty;
            try
            {
                result = items.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new GroupInfo
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
                    result = Config.Serializer.Deserialize<GroupInfo>(serializer);
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



        public DataTable Search(GeneralSimpleGroupSearchView searchView, MiicPage page)
        {
            Contract.Requires<ArgumentNullException>(searchView != null, "参数searchView:不能为空");
            DataTable result = new DataTable();
            string message = string.Empty;
            MiicConditionCollections conditions = searchView.visitor(this);
            List<MiicOrderBy> order = new List<MiicOrderBy>();
            order.Add(new MiicOrderBy()
            {
                Desc = false,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.Name)
            });
            order.Add(new MiicOrderBy()
            {
                Desc = true,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, int?>(o => o.MemberCount)
            });
            conditions.order = order;
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<GroupInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.ID),
                Config.Attribute.GetSqlTableNameByClassName<GroupMember>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.GroupID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.InnerJoin);
            MiicColumnCollections columns = new MiicColumnCollections();
            MiicColumn groupInfoColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<GroupInfo>());
            columns.Add(groupInfoColumn);
            MiicColumn remarkColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<GroupMember>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.Remark));
            columns.Add(remarkColumn);
            MiicColumn groupMemberIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<GroupMember>(),
                string.Empty,
                Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.ID),
                "GroupMemberID");
            columns.Add(groupMemberIDColumn);
            try
            {
                if (page != null)
                {
                    result = dbService.GetInformationsPage(columns, relation, conditions, page, out message);
                }
                else
                {
                    result = dbService.GetInformations(columns, relation, conditions, out message);
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

        public int GetSearchCount(GeneralSimpleGroupSearchView searchView)
        {
            Contract.Requires<ArgumentNullException>(searchView != null, "参数searchView:不能为空");
            int result = 0;
            MiicConditionCollections conditions = searchView.visitor(this);
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<GroupInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.ID),
               Config.Attribute.GetSqlTableNameByClassName<GroupMember>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.GroupID),
               MiicDBOperatorSetting.Equal,
               MiicDBRelationSetting.InnerJoin);
            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<GroupInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.ID));
            string message = string.Empty;
            try
            {
                result = dbService.GetCount(column, relation, conditions, out message);
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


        public bool Insert(GroupInfo groupInfo, List<GroupMember> members)
        {
            Contract.Requires<ArgumentNullException>(groupInfo != null && !string.IsNullOrEmpty(groupInfo.ID), "参数groupInfo:不能为空");
            bool result = false;
            int count = 0;
            string message1 = string.Empty;
            string message2 = string.Empty;
            string message = string.Empty;
            List<string> sqls = new List<string>();
            foreach (GroupMember item in members)
            {
                sqls.Add(DBService.InsertSql(item, out message2));
            }
            //管理员
            GroupMember member = new GroupMember()
            {
                ID = Guid.NewGuid().ToString(),
                GroupID = groupInfo.ID,
                JoinTime = DateTime.Now,
                MemberID = groupInfo.CreaterID,
                MemberName = groupInfo.CreaterName
            };
            sqls.Add(DBService.InsertSql(member, out message2));
            sqls.Add(DBService.InsertSql(groupInfo, out message2));
            try
            {
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required))
                {
                    try
                    {
                        bool result1 = dbService.excuteSqls(sqls, out message);
                        bool result2 = false;
                        if (result1 == true)
                        {
                            groupInfo.LogoUrl = this.CreateGroupUrl(groupInfo.ID);
                            if (!string.IsNullOrEmpty(groupInfo.LogoUrl))
                            {
                                result2 = dbService.Update(groupInfo, out count, out message1);
                            }
                        }
                        if (result1 == true && result2 == true)
                        {
                            result = true;
                            ts.Complete();
                        }
                    }
                    catch (Exception ex)
                    {
                        ts.Dispose();
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
                InsertCache(groupInfo, members);
            }
            return result;
        }


        public DataTable GetGroupInfoList(string userID, int top)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(userID), "参数userID:不能为空");
            DataTable result = new DataTable();
            string message = string.Empty;
            MiicConditionCollections conditions = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition userIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.MemberID),
                userID,
                DbType.String,
               MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, userIDCondition));
            MiicCondition validCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.Valid),
              ((int)MiicValidTypeSetting.Valid).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(validCondition));
            List<MiicOrderBy> orders = new List<MiicOrderBy>();
            orders.Add(new MiicOrderBy()
            {
                Desc = true,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<GroupInfo, int?>(o => o.SortNo)
            });
            conditions.order = orders;
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<GroupInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.ID),
                Config.Attribute.GetSqlTableNameByClassName<GroupMember>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.GroupID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.InnerJoin);
            MiicColumnCollections columns = new MiicColumnCollections(new MiicTop(top));
            MiicColumn groupIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<GroupInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.ID));
            columns.Add(groupIDColumn);
            MiicColumn groupNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<GroupInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.Name));
            columns.Add(groupNameColumn);
            MiicColumn logoUrlColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<GroupInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.LogoUrl));
            columns.Add(logoUrlColumn);
            MiicColumn remarkColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<GroupMember>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.Remark));
            columns.Add(remarkColumn);
            try
            {
                result = dbService.GetInformations(columns, relation, conditions, out message);
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

        public bool HaveGroup(string userID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(userID), "参数userID:不能为空");
            bool result = false;
            int count;
            string message = string.Empty;
            MiicConditionCollections conditions = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition userIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.MemberID),
                userID,
                DbType.String,
               MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, userIDCondition));
            MiicCondition validCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.Valid),
              ((int)MiicValidTypeSetting.Valid).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(validCondition));
            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<GroupInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.ID));
             
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<GroupInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.ID),
                Config.Attribute.GetSqlTableNameByClassName<GroupMember>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.GroupID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.InnerJoin);
            
            try
            {
                count = dbService.GetCount(column, relation, conditions, out message);
                if (count != 0&& count!=null)
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
        /// 生成并设置小组头像
        /// </summary>
        /// <param name="groupID">小组ID</param>
        /// <returns>Yes/No</returns>
        private string CreateGroupUrl(string groupID)
        {
            string result = string.Empty;
            string message = string.Empty;
            int memberCount = 0;
            List<string> memberUrlList = new List<string>();
            MiicCondition groupIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.GroupID),
                groupID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            MiicConditionSingle condition = new MiicConditionSingle(groupIDCondition);
            List<MiicOrderBy> order = new List<MiicOrderBy>();
            MiicOrderBy sortOrder = new MiicOrderBy()
            {
                Desc = false,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<GroupMember, int?>(o => o.SortNo)
            };
            order.Add(sortOrder);
            condition.order = order;
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<GroupMember>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.MemberID),
               Config.Attribute.GetSqlTableNameByClassName<MiicSocialUserInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<MiicSocialUserInfo, string>(o => o.ID),
               MiicDBOperatorSetting.Equal,
               MiicDBRelationSetting.InnerJoin);
            MiicColumnCollections columns = new MiicColumnCollections(new MiicTop(9));
            MiicColumn urlColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<MiicSocialUserInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<MiicSocialUserInfo, string>(o => o.MicroUserUrl));
            columns.Add(urlColumn);
                //取小组前9人的头像
                DataTable memberTable = dbService.GetInformations(columns, relation, condition, out message);
                GroupInfo temp = ((ICommon<GroupInfo>)this).GetInformation(groupID);
                memberCount = memberTable.Rows.Count;
                if (memberCount != 0)
                {
                    DrawService drawService = new DrawService();
                    drawService.SaveFile = GroupLogoPath;
                    foreach (var dr in memberTable.AsEnumerable())
                    {
                        memberUrlList.Add(ManageUrl + dr[Config.Attribute.GetSqlColumnNameByPropertyName<MiicSocialUserInfo, string>(o => o.MicroUserUrl)].ToString());
                    }
                    result = drawService.CreateImageByDiscussionGroupTypeByUrl(memberUrlList);

                    if (result != string.Empty)
                    {
                        //删除原来的组头像
                        if (temp != null && !string.IsNullOrEmpty(temp.LogoUrl))
                        {
                            File.Delete(HttpContext.Current.Server.MapPath(GroupLogoPath+ Path.GetFileName(temp.LogoUrl)));
                        }
                    }
                    else
                    {
                        throw new Exception("生成组头像异常");
                    }
                }
            return result;
        }

        DataTable IGroupInfo.TrendsSearch(GeneralSimpleGroupSearchView searchView, MiicPage page)
        {
            Contract.Requires<ArgumentNullException>(searchView != null, "参数searchView:不能为空");
            DataTable result = new DataTable();
            string message = string.Empty;
            try
            {
                if (page == null)
                {
                    Dictionary<String, String> paras = new Dictionary<String, String>();
                    paras.Add("KEYWORD", searchView.Keyword);
                    paras.Add("USER_ID", ((MySimpleGroupSearchView)searchView).UserID);
                    paras.Add("PAGE_START", string.Empty);
                    paras.Add("PAGE_END", string.Empty);
                    string storeProcedureName = "SearchGroupTrends";
                    result = dbService.QueryStoredProcedure<string>(storeProcedureName, paras, out message);
                }
                else
                {
                    Dictionary<String, String> paras = new Dictionary<String, String>();
                    paras.Add("KEYWORD", searchView.Keyword);
                    paras.Add("USER_ID", ((MySimpleGroupSearchView)searchView).UserID);
                    paras.Add("PAGE_START", page.pageStart);
                    paras.Add("PAGE_END", page.pageEnd);
                    string storeProcedureName = "SearchGroupTrends";
                    result = dbService.QueryStoredProcedure<string>(storeProcedureName, paras, out message);
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

        int IGroupInfo.GetTrendsSearchCount(GeneralSimpleGroupSearchView searchView)
        {
            Contract.Requires<ArgumentNullException>(searchView != null, "参数searchView:不能为空");
            int result = 0;
            MiicConditionCollections conditions = searchView.visitor(this);
            MiicCondition topicValidCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<TopicInfo, string>(o => o.Valid),
                ((int)MiicValidTypeSetting.Valid).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(topicValidCondition));
            MiicRelationCollections relations = new MiicRelationCollections(Config.Attribute.GetSqlTableNameByClassName<GroupInfo>());
            MiicFriendRelation memberRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.ID),
               Config.Attribute.GetSqlTableNameByClassName<GroupMember>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.GroupID),
               MiicDBOperatorSetting.Equal,
               MiicDBRelationSetting.InnerJoin);
            relations.Add(memberRelation);
            MiicFriendRelation topicRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.ID),
                Config.Attribute.GetSqlTableNameByClassName<TopicInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<TopicInfo, string>(o => o.GroupID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.InnerJoin);
            relations.Add(topicRelation);
            //MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<GroupInfo>(),
            //    Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.ID));
            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<TopicInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<TopicInfo, string>(o => o.ID));
            string message = string.Empty;
            try
            {
                result = dbService.GetCount(column, relations, conditions, out message, true);
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

        bool IGroupInfo.IsCreater(string userID, string groupID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(userID), "参数userID:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(groupID), "参数groupID:不能为空");
            bool result = false;
            string message = string.Empty;
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition createrCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.CreaterID),
               userID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, createrCondition));
            MiicCondition groupIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.ID),
                       groupID,
                       DbType.String,
                       MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(groupIDCondition));
            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<GroupInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.ID));
            try
            {
                int count = dbService.GetCount<GroupInfo>(column, condition, out message);
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

        DataTable IGroupInfo.GetDetailGroupByTopicID(string topicID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(topicID), "参数topicID:不能为空");
            DataTable result = new DataTable();
            string message = string.Empty;
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition topicIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<TopicInfo, string>(o => o.ID),
               topicID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, topicIDCondition));
            MiicCondition validCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<TopicInfo, string>(o => o.Valid),
                       ((int)MiicValidTypeSetting.Valid).ToString(),
                       DbType.String,
                       MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(validCondition));

            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<TopicInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<TopicInfo, string>(o => o.GroupID),
                Config.Attribute.GetSqlTableNameByClassName<GroupInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<GroupInfo, string>(o => o.ID),
                 MiicDBOperatorSetting.Equal,
                  MiicDBRelationSetting.LeftJoin);
            MiicColumnCollections column = new MiicColumnCollections();
            MiicColumn groupColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<GroupInfo>());
            column.Add(groupColumn);
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
    }

}
