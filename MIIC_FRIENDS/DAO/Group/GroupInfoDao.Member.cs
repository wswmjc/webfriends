using Miic.Base;
using Miic.DB;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.Common.Setting;
using Miic.Friends.Notice.Setting;
using Miic.Friends.SimpleGroup;
using Miic.Log;
using Miic.Manage.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Transactions;
namespace Miic.Friends.Group
{
    public partial class GroupInfoDao : RelationCommon<GroupInfo, GroupMember>, IGroupInfo
    {
        private object syncRoot = new object();
        bool ICommon<GroupMember>.Insert(GroupMember member)
        {
            Contract.Requires<ArgumentNullException>(member != null, "参数member:不能为空");
            bool result = false;
            int count = 0;
            List<string> sqls = new List<string>();
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            GroupInfo temp = ((ICommon<GroupInfo>)this).GetInformation(member.GroupID);
            string tempUrl = string.Empty;
            if (temp != null)
            {
                try
                {
                    if (temp.MemberCount < 9)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required))
                        {
                            bool result1 = dbService.Insert(member, out count, out message1);
                            bool result2 = false;
                            if (result1 == true)
                            {
                                GroupInfo updateTemp = new GroupInfo()
                                {
                                    ID = temp.ID,
                                    MemberCount = temp.MemberCount + 1,
                                    LogoUrl = this.CreateGroupUrl(temp.ID)
                                };
                                tempUrl = updateTemp.LogoUrl;
                                result2 = dbService.Update(temp, out count, out message2);
                            }
                            if (result1 == true && result2 == true)
                            {
                                result = true;
                                ts.Complete();
                            }
                        }
                    }
                    else
                    {
                        sqls.Add(DBService.InsertSql(member, out message1));
                        sqls.Add(DBService.UpdateSql(new GroupInfo()
                            {
                                ID = temp.ID,
                                MemberCount = temp.MemberCount + 1
                            }, out message2));
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
            if (result == true)
            {
                List<GroupMember> cache = new List<GroupMember>();
                cache.Add(member);
                InsertCaches(cache);
                lock (syncRoot)
                {
                    if (items.Find(o => o.ID == member.GroupID) != null)
                    {
                        items.Find(o => o.ID == member.GroupID).MemberCount = temp.MemberCount + 1;
                        if (!string.IsNullOrEmpty(tempUrl))
                        {
                            items.Find(o => o.ID == member.GroupID).LogoUrl = tempUrl;
                        }
                    }
                }
            }
            return result;
        }

        bool ICommon<GroupMember>.Update(GroupMember member)
        {
            Contract.Requires<ArgumentNullException>(member != null, "参数member:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            try
            {
                result = dbService.Update(member, out count, out message);
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
                DeleteCache(o => o.ID == member.ID);
            }
            return result;
        }

        bool ICommon<GroupMember>.Delete(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            bool result = false;
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            int count = 0;
            List<string> sqls = new List<string>();
            GroupInfo tempGroup = null;
            string tempUrl = string.Empty;
            try
            {
                GroupMember tempMember = ((ICommon<GroupMember>)this).GetInformation(id);
                if (tempMember != null)
                {
                    tempGroup = ((ICommon<GroupInfo>)this).GetInformation(tempMember.GroupID);
                    if (tempGroup != null)
                    {
                        if (tempGroup.MemberCount <= 9)
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required))
                            {
                                try
                                {
                                    bool result1 = dbService.Delete(new GroupMember()
                                    {
                                        ID = id
                                    }, out count, out message1);
                                    bool result2 = false;
                                    if (result1 == true)
                                    {
                                        GroupInfo updateTemp = new GroupInfo()
                                        {
                                            ID = tempGroup.ID,
                                            MemberCount = tempGroup.MemberCount - 1,
                                            LogoUrl = this.CreateGroupUrl(tempGroup.ID)
                                        };
                                        tempUrl = updateTemp.LogoUrl;
                                        result2 = dbService.Update(updateTemp, out count, out message2);
                                    }
                                    //删除@提醒 Topic
                                    sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Group).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.PublishInfo).ToString() + "' and NOTICER_ID='" + tempMember.MemberID + "' and PUBLISH_ID in (select ID from GROUP_TOPIC_INFO where GROUP_ID='" + tempMember.GroupID + "') ");
                                    //删除@提醒 Message
                                    sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Group).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.Message).ToString() + "' and NOTICER_ID='" + tempMember.MemberID + "' and PUBLISH_ID in (select ID from GROUP_MESSAGE_INFO where TOPIC_ID IN (select ID from GROUP_TOPIC_INFO where GROUP_ID='" + tempMember.GroupID + "'))");
                                    bool result3 = dbService.excuteSqls(sqls, out message);
                                    if (result1 == true && result2 == true && result3 == true)
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
                        else
                        {
                            sqls.Add(DBService.DeleteSql(new GroupMember()
                            {
                                ID = id
                            }, out message1));
                            sqls.Add(DBService.UpdateSql(new GroupInfo()
                                {
                                    ID = tempGroup.ID,
                                    MemberCount = tempGroup.MemberCount - 1
                                }, out message2));
                            //删除@提醒 Topic
                            sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Group).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.PublishInfo).ToString() + "' and NOTICER_ID='" + tempMember.MemberID + "' and PUBLISH_ID in (select ID from GROUP_TOPIC_INFO where GROUP_ID='" + tempMember.GroupID + "') ");
                            //删除@提醒 Message
                            sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Group).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.Message).ToString() + "' and NOTICER_ID='" + tempMember.MemberID + "' and PUBLISH_ID in (select ID from GROUP_MESSAGE_INFO where TOPIC_ID IN (select ID from GROUP_TOPIC_INFO where GROUP_ID='" + tempMember.GroupID + "'))");
                            result = dbService.excuteSqls(sqls, out message);
                        }
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
                lock (syncRoot)
                {
                    if (items.Find(o => o.ID == tempGroup.ID) != null)
                    {
                        items.Find(o => o.ID == tempGroup.ID).MemberCount = tempGroup.MemberCount - 1;
                        if (!string.IsNullOrEmpty(tempUrl))
                        {
                            items.Find(o => o.ID == tempGroup.ID).LogoUrl = tempUrl;
                        }
                    }
                }
            }
            return result;
        }

        GroupMember ICommon<GroupMember>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            GroupMember result = null;
            string message = string.Empty;
            try
            {
                result = subitems.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new GroupMember
                    {
                        ID = id
                    }, out message);
                    if (result != null)
                    {
                        List<GroupMember> cache = new List<GroupMember>();
                        cache.Add(result);
                        InsertCaches(cache);
                    }
                }
                else
                {
                    string serializer = Config.Serializer.Serialize(result);
                    result = Config.Serializer.Deserialize<GroupMember>(serializer);
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

        public bool Delete(string groupID, List<string> memberIDs)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(groupID), "参数groupID:不能为空");
            Contract.Requires<ArgumentNullException>(memberIDs != null && memberIDs.Count != 0, "参数memberIDs:不能为空");
            bool result = false;
            int count = 0;
            List<string> sqls = new List<string>();
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            string tempUrl = string.Empty;
            string members = string.Empty;
           
            foreach (var item in memberIDs)
            {
                members += "'" + item + "',";
                MiicConditionCollections condition = new MiicConditionCollections();
                MiicCondition memberIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.MemberID),
                    item,
                    DbType.String,
                    MiicDBOperatorSetting.Equal);
                condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, memberIDCondition));
                MiicCondition groupIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.GroupID),
                    groupID,
                    DbType.String,
                    MiicDBOperatorSetting.Equal);
                condition.Add(new MiicConditionLeaf(groupIDCondition));
                sqls.Add(DBService.DeleteConditionsSql<GroupMember>(condition,out message1));
            }
            members = members.TrimEnd(',');
            try
            {
                GroupInfo tempGroup = ((ICommon<GroupInfo>)this).GetInformation(groupID);
                if (tempGroup != null)
                {
                    if ((tempGroup.MemberCount - memberIDs.Count) < 9)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required))
                        {
                            try
                            {
                                bool result1 = dbService.excuteSqls(sqls, out message);
                                bool result2 = false;
                                if (result1 == true)
                                {
                                    GroupInfo updateTemp = new GroupInfo()
                                        {
                                            ID = tempGroup.ID,
                                            MemberCount = tempGroup.MemberCount - memberIDs.Count,
                                            LogoUrl = this.CreateGroupUrl(tempGroup.ID)
                                        };
                                    tempUrl = updateTemp.LogoUrl;
                                    result2 = dbService.Update(updateTemp, out count, out message);


                                }
                                //删除@提醒 Topic
                                sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Group).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.PublishInfo).ToString() + "' and NOTICER_ID in(" + members + ") and PUBLISH_ID in (select ID from GROUP_TOPIC_INFO where GROUP_ID='" + groupID + "') ");
                                //删除@提醒 Message
                                sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Group).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.Message).ToString() + "' and NOTICER_ID in(" + members + ") and PUBLISH_ID in (select ID from GROUP_MESSAGE_INFO where TOPIC_ID IN (select ID from GROUP_TOPIC_INFO where GROUP_ID='" + groupID + "'))");
                                bool result3 = dbService.excuteSqls(sqls, out message);
                                if (result1 == true && result2 == true && result3 == true)
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
                    else
                    {
                        sqls.Add(DBService.UpdateSql(new GroupInfo()
                        {
                            ID = tempGroup.ID,
                            MemberCount = tempGroup.MemberCount - memberIDs.Count
                        }, out message));
                        //删除@提醒 Topic
                      
                        sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Group).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.PublishInfo).ToString() + "' and NOTICER_ID in(" + members + ") and PUBLISH_ID in (select ID from GROUP_TOPIC_INFO where GROUP_ID='" + groupID + "') ");
                        //删除@提醒 Message
                        sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Group).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.Message).ToString() + "' and NOTICER_ID in(" + members + ") and PUBLISH_ID in (select ID from GROUP_MESSAGE_INFO where TOPIC_ID IN (select ID from GROUP_TOPIC_INFO where GROUP_ID='" + groupID + "'))");
                        result = dbService.excuteSqls(sqls, out message);
                    }
                }
                else
                {
                    throw new Exception("参数错误，未知的讨论组");
                }

                if (result == true)
                {
                    foreach (string item in memberIDs)
                    {
                        DeleteCache(o => o.MemberID == item && o.GroupID == groupID);
                    }
                    lock (syncRoot)
                    {
                        if (tempGroup != null)
                        {
                            items.Find(o => o.ID == tempGroup.ID).MemberCount = tempGroup.MemberCount - memberIDs.Count;
                            if (!string.IsNullOrEmpty(tempUrl))
                            {
                                items.Find(o => o.ID == tempGroup.ID).LogoUrl = tempUrl;
                            }
                        }
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
            return result;
        }

        public bool Insert(string groupID, List<GroupMember> members)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(groupID), "参数groupID:不能为空");
            Contract.Requires<ArgumentNullException>(members != null && members.Count != 0, "参数members:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            List<string> sqls = new List<string>();
            string tempUrl = string.Empty;
            try
            {
                GroupInfo temp = ((ICommon<GroupInfo>)this).GetInformation(groupID);
                if (temp != null)
                {
                    foreach (GroupMember item in members)
                    {
                        sqls.Add(DBService.InsertSql(new GroupMember()
                        {
                            ID = Guid.NewGuid().ToString(),
                            GroupID = groupID,
                            MemberID = item.MemberID,
                            MemberName = item.MemberName,
                            JoinTime = DateTime.Now
                        }, out message1));
                    }

                    if (temp.MemberCount < 9)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required))
                        {
                            try
                            {
                                bool result1 = dbService.excuteSqls(sqls, out message);
                                bool result2 = false;
                                if (result1 == true)
                                {
                                    GroupInfo updateTemp = new GroupInfo()
                                    {
                                        ID = groupID,
                                        MemberCount = members.Count + temp.MemberCount,
                                        LogoUrl = this.CreateGroupUrl(groupID)
                                    };
                                    tempUrl = updateTemp.LogoUrl;
                                    result2 = dbService.Update(updateTemp, out count, out message);
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
                    else
                    {
                        sqls.Add(DBService.UpdateSql(new GroupInfo()
                                {
                                    ID = groupID,
                                    MemberCount = members.Count + temp.MemberCount
                                }, out message2));
                        result = dbService.excuteSqls(sqls, out message);
                    }
                }
                else
                {
                    throw new Exception("参数错误，未知的讨论组");
                }

                if (result == true)
                {
                    lock (syncRoot)
                    {
                        if (items.Find(o => o.ID == groupID) != null)
                        {
                            items.Find(o => o.ID == groupID).MemberCount = temp.MemberCount + members.Count;
                            if (!string.IsNullOrEmpty(tempUrl))
                            {
                                items.Find(o => o.ID == groupID).LogoUrl = tempUrl;
                            }
                        }
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
            return result;
        }

        public bool SetRemark(SetRemarkView remarkView)
        {
            Contract.Requires<ArgumentNullException>(remarkView != null, "参数remarkView:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            try
            {
                result = dbService.Update(new GroupMember()
                {
                    ID = remarkView.ID,
                    Remark = remarkView.Remark
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
                DeleteCache(o => o.ID == remarkView.ID);
            }
            return result;
        }

        /// <summary>
        /// 根据讨论组ID获取所有讨论组成员
        /// </summary>
        /// <param name="groupID">讨论组ID</param>
        /// <returns>讨论组成员列表</returns>
        List<GroupMember> IGroupInfo.GetGroupMemberList(string groupID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(groupID), "参数groupID:不能为空");
            List<GroupMember> result = new List<GroupMember>();
            MiicCondition communityIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.GroupID),
                groupID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            string message = string.Empty;
            MiicColumn allColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<GroupMember>());
            MiicColumnCollections columns = new MiicColumnCollections();
            columns.Add(allColumn);
            MiicConditionSingle condition = new MiicConditionSingle(communityIDCondition);
            try
            {
                DataTable dt = dbService.GetInformations<GroupMember>(columns, condition, out message);
                if (dt.Rows.Count != 0)
                {
                    foreach (DataRow item in dt.AsEnumerable())
                    {
                        result.Add(new GroupMember()
                        {
                            ID = item[Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.ID)].ToString(),
                            MemberID = item[Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.MemberID)].ToString(),
                            MemberName = item[Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.MemberName)].ToString(),
                            GroupID = item[Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.GroupID)].ToString(),
                            JoinTime = (DateTime?)item[Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, DateTime?>(o => o.JoinTime)],
                            Remark = item[Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.Remark)].ToString()
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
            return result;
        }

        DataTable IGroupInfo.GetDetailMemberInfoListByGroupID(string groupID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(groupID), "参数groupID:不能为空");
            string message = string.Empty;
            DataTable result = new DataTable();
            MiicCondition groupIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.GroupID),
                groupID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            MiicConditionSingle condition = new MiicConditionSingle(groupIDCondition);

            MiicColumnCollections column = new MiicColumnCollections();
            MiicColumn memberAll = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<GroupMember>());
            column.Add(memberAll);
            MiicColumn userUrl = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<Miic.Friends.User.SimpleUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.User.SimpleUserView, string>(o => o.UserUrl));
            column.Add(userUrl);
            MiicColumn userName = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<Miic.Friends.User.SimpleUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.User.SimpleUserView, string>(o => o.UserName));
            column.Add(userName);
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<GroupMember>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<GroupMember, string>(o => o.MemberID),
                Config.Attribute.GetSqlTableNameByClassName<Miic.Friends.User.SimpleUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.User.SimpleUserView, string>(o => o.UserID),
                 MiicDBOperatorSetting.Equal,
                  MiicDBRelationSetting.InnerJoin);
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


        DataTable IGroupInfo.GetInvitingAddressList(MySimpleGroupSearchView groupSearchView, MiicPage page)
        {
            Contract.Requires<ArgumentNullException>(groupSearchView != null, "参数groupSearchView:不能为空");
            string message = string.Empty;
            string sql = string.Empty;
            DataTable result = new DataTable();
            sql += "SELECT ADDRESSER_ID,ADDRESSER_NAME,REMARK,USER_NAME,MICRO_USER_URL FROM ADDRESS_BOOK_INFO";
            sql += " LEFT JOIN SIMPLE_USER_VIEW ON ADDRESS_BOOK_INFO.ADDRESSER_ID = SIMPLE_USER_VIEW.USER_ID ";
            sql += " WHERE MY_USER_ID = '" + groupSearchView.UserID + "' AND ";
            sql += " ADDRESSER_ID NOT IN (SELECT MEMBER_ID FROM GROUP_MEMBER WHERE GROUP_ID = '" + groupSearchView.GroupID + "') ";
            sql += " AND (ADDRESSER_NAME LIKE '%" + groupSearchView.Keyword + "%' OR USER_NAME LIKE '%" + groupSearchView.Keyword + "%')";

            if (page != null)
            {
                sql = "WITH LIST_PAGE AS ( SELECT ROW_NUMBER() OVER (ORDER BY TEMP.USER_NAME ASC) AS row, TEMP.* FROM ( " + sql;
                sql += " ) AS TEMP ) ";
                sql += " SELECT * FROM LIST_PAGE WHERE row BETWEEN " + page.pageStart + " AND " + page.pageEnd;
            }

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

        int IGroupInfo.GetInvitingAddresserCount(MySimpleGroupSearchView groupSearchView)
        {
            Contract.Requires<ArgumentNullException>(groupSearchView != null, "参数groupSearchView:不能为空");
            string message = string.Empty;
            string sql = string.Empty;
            int result = 0;
            sql += "SELECT COUNT(ADDRESSER_ID) FROM ADDRESS_BOOK_INFO";
            sql += " LEFT JOIN SIMPLE_USER_VIEW ON ADDRESS_BOOK_INFO.ADDRESSER_ID = SIMPLE_USER_VIEW.USER_ID ";
            sql += " WHERE MY_USER_ID = '" + groupSearchView.UserID + "' AND ";
            sql += " ADDRESSER_ID NOT IN (SELECT MEMBER_ID FROM GROUP_MEMBER WHERE GROUP_ID = '" + groupSearchView.GroupID + "') ";
            sql += " AND (ADDRESSER_NAME LIKE '%" + groupSearchView.Keyword + "%' OR USER_NAME LIKE '%" + groupSearchView.Keyword + "%')";
            try
            {
                result = dbService.GetSqlCount(sql, out message);
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
