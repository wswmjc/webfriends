using Miic.Base;
using Miic.Base.Setting;
using Miic.DB;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.AddressBook;
using Miic.Friends.Common.Setting;
using Miic.Friends.Community.Setting;
using Miic.Friends.Notice.Setting;
using Miic.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Miic.Friends.Community
{
    public partial class CommunityInfoDao : RelationCommon<CommunityInfo, CommunityMember>, ICommunityInfo
    {
        private object syncRoot = new object();
        bool ICommon<CommunityMember>.Insert(CommunityMember member)
        {
            Contract.Requires<ArgumentNullException>(member != null, "参数member：不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(member.ID), "参数member.ID：不能为空！");
            bool result = false;
            string message = string.Empty;
            string message1 = string.Empty;
            List<string> sqls = new List<string>();
            sqls.Add(DBService.InsertSql(member, out message1));
            CommunityInfo commuityInfo = ((ICommon<CommunityInfo>)this).GetInformation(member.CommunityID);
            sqls.Add(DBService.UpdateSql(new CommunityInfo()
            {
                ID = member.CommunityID,
                MemberCount = commuityInfo.MemberCount + 1
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
                    List<CommunityMember> memberList = new List<CommunityMember>();
                    memberList.Add(member);
                    InsertCaches(memberList);
                    if (items.Find(o => o.ID == commuityInfo.ID) != null)
                    {
                        items.Find(o => o.ID == commuityInfo.ID).MemberCount = commuityInfo.MemberCount + 1;
                    }
                }
            }
            return result;
        }

        bool ICommon<CommunityMember>.Update(CommunityMember member)
        {
            Contract.Requires<ArgumentNullException>(member != null, "参数member:不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(member.ID), "参数member.ID:不能为空，因为是主键");
            int count = 0;
            string message = string.Empty;
            bool result = false;
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

        bool ICommon<CommunityMember>.Delete(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            bool result = false;
            string message = string.Empty;
            CommunityMember communityMember = ((ICommon<CommunityMember>)this).GetInformation(id);
            CommunityInfo commuityInfo = ((ICommon<CommunityInfo>)this).GetInformation(communityMember.CommunityID);
            try
            {
                List<string> sqls = new List<string>();
                sqls.Add(DBService.UpdateSql<CommunityInfo>(new CommunityInfo()
                {
                    ID = communityMember.CommunityID,
                    MemberCount = commuityInfo.MemberCount - 1
                }, out message));
                sqls.Add(DBService.DeleteSql<CommunityMember>(new CommunityMember()
                {
                    ID = id
                }, out message));
                //删除@提醒 Topic
                sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Community).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.PublishInfo).ToString() + "' and NOTICER_ID='" + communityMember.MemberID + "' and PUBLISH_ID in (select ID from COMMUNITY_TOPIC_INFO where COMMUNITY_ID='" + communityMember.CommunityID + "') ");
                sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Community).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.PublishInfo).ToString() + "' and NOTICER_ID='" + communityMember.MemberID + "' and PUBLISH_ID in (select ID from COMMUNITY_PUBLISH_LABEL_RELATION where COMMUNITY_ID='" + communityMember.CommunityID + "') ");
                //删除@提醒 Message
                sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Community).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.Message).ToString() + "' and NOTICER_ID='" + communityMember.MemberID + "' and PUBLISH_ID in (select ID from COMMUNITY_MESSAGE_INFO where TOPIC_ID=(select ID from COMMUNITY_TOPIC_INFO where COMMUNITY_ID='" + communityMember.CommunityID + "'))");
                sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Community).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.Message).ToString() + "'  and NOTICER_ID='" + communityMember.MemberID + "' and PUBLISH_ID in (select ID from COMMUNITY_COMMENT_INFO where PUBLISH_ID in(select ID from COMMUNITY_PUBLISH_LABEL_RELATION where COMMUNITY_ID='" + communityMember.CommunityID + "'))");
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
                DeleteCache(o => o.ID == id);
                lock (syncRoot)
                {
                    items.Find(o => o.ID == commuityInfo.ID).MemberCount = commuityInfo.MemberCount - 1;
                }
            }
            return result;
        }

        CommunityMember ICommon<CommunityMember>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            CommunityMember result = null;
            string message = string.Empty;
            try
            {
                result = subitems.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new CommunityMember
                    {
                        ID = id
                    }, out message);
                    if (result != null)
                    {
                        List<CommunityMember> memberList = new List<CommunityMember>();
                        memberList.Add(result);
                        InsertCaches(memberList);
                    }
                }
                else
                {
                    string serializer = Config.Serializer.Serialize(result);
                    result = Config.Serializer.Deserialize<CommunityMember>(serializer);
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

        bool ICommunityInfo.MemberApply(List<CommunityApplicationInfo> memberApplications)
        {
            Contract.Requires<ArgumentNullException>(memberApplications != null && memberApplications.Count != 0, "参数memberApplications:不能为空");
            string message = string.Empty;
            bool result = false;
            foreach (CommunityApplicationInfo item in memberApplications.AsEnumerable())
            {
                item.ResponseStatus = ((int)ApplyStatusSetting.Apply).ToString();
                item.ApplicationTime = DateTime.Now;
                if (string.IsNullOrEmpty(item.Remark))
                {
                    item.Remark = item.MemberName + "申请加入您的行业圈子";
                }
            }

            try
            {
                result = dbService.Inserts(memberApplications, out message);
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
        bool ICommunityInfo.MemberInvite(List<CommunityApplicationInfo> memberApplications)
        {
            Contract.Requires<ArgumentNullException>(memberApplications != null && memberApplications.Count != 0, "参数memberApplications:不能为空");
            string message = string.Empty;
            bool result = false;
            foreach (CommunityApplicationInfo item in memberApplications.AsEnumerable())
            {
                item.ResponseStatus = ((int)ApplyStatusSetting.Invite).ToString();
                item.ApplicationTime = DateTime.Now;
                if (string.IsNullOrEmpty(item.Remark))
                {
                    item.Remark = item.MemberName + "申请加入您的行业圈子";
                }
            }

            try
            {
                result = dbService.Inserts(memberApplications, out message);
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
        /// 批量删除成员
        /// </summary>
        /// <param name="communityID">圈子ID</param>
        /// <param name="memberIDs">成员ID</param>
        /// <returns></returns>
        public bool RemoveMembers(string communityID, List<string> memberIDs)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(communityID), "参数communityID:不能为空");
            Contract.Requires<ArgumentNullException>(memberIDs != null && memberIDs.Count != 0, "参数memberIDs:不能为空");
            bool result = false;
            List<string> sqls = new List<string>();
            string message = string.Empty;
            string members = string.Empty;
            CommunityInfo commuityInfo = ((ICommon<CommunityInfo>)this).GetInformation(communityID);
            try
            {
                sqls.Add(DBService.UpdateSql<CommunityInfo>(new CommunityInfo()
                {
                    ID = communityID,
                    MemberCount = commuityInfo.MemberCount - memberIDs.Count
                }, out message));

                foreach (var item in memberIDs)
                {
                    members += "'" + item + "',";
                    sqls.Add(DBService.DeleteSql<CommunityMember>(new CommunityMember()
                    {
                        ID = item
                    }, out message));
                }
                members = members.TrimEnd(',');

                //删除@提醒 Topic
                sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Community).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.PublishInfo).ToString() + "'  and NOTICER_ID in(" + members + ") and PUBLISH_ID in (select ID from COMMUNITY_TOPIC_INFO where COMMUNITY_ID='" + communityID + "') ");
                sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Community).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.PublishInfo).ToString() + "'  and NOTICER_ID in(" + members + ") and PUBLISH_ID in (select ID from COMMUNITY_PUBLISH_LABEL_RELATION where COMMUNITY_ID='" + communityID + "') ");
                //删除@提醒 Message
                sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Community).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.Message).ToString() + "'  and NOTICER_ID in(" + members + ") and PUBLISH_ID in (select ID from COMMUNITY_MESSAGE_INFO where TOPIC_ID=(select ID from COMMUNITY_TOPIC_INFO where COMMUNITY_ID='" + communityID + "'))");
                sqls.Add("delete from NOTICE_INFO where SOURCE='" + ((int)BusinessTypeSetting.Community).ToString() + "' and NOTICE_TYPE='" + ((int)NoticeTypeSetting.Message).ToString() + "'  and NOTICER_ID in(" + members + ") and PUBLISH_ID in (select ID from COMMUNITY_COMMENT_INFO where PUBLISH_ID in(select ID from COMMUNITY_PUBLISH_LABEL_RELATION where COMMUNITY_ID='" + communityID + "'))");
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
                    foreach (var item in memberIDs)
                    {
                        DeleteCache(o => o.ID == item);
                    }
                    if (items.Find(o => o.ID == commuityInfo.ID) != null)
                    {
                        items.Find(o => o.ID == commuityInfo.ID).MemberCount = commuityInfo.MemberCount - memberIDs.Count;
                    }
                }
            }
            return result;
        }
        int ICommunityInfo.GetPersonValidationMessageCount(string userID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(userID), "参数userID:不能为空");
            string message = string.Empty;
            int result = 0;
            try
            {
                result = dbService.GetCount("select * from GetPersonCommunityValidationMessageInfo('" + userID + "')", out message);
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


        DataTable ICommunityInfo.GetPersonValidationMessageInfos(string userID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(userID), "参数userID:不能为空");
            string message = string.Empty;
            DataTable result = new DataTable();
            try
            {
                result = dbService.querySql("select * from GetPersonCommunityValidationMessageInfo('" + userID + "')", out message);
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


        bool ICommunityInfo.IsAdmin(string userID, string communityID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(userID), "参数userID:不能为空");
            bool result = false;
            string message = string.Empty;
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition isAdminCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.IsAdmin),
                ((int)MiicYesNoSetting.Yes).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, isAdminCondition));
            MiicCondition userIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.MemberID),
                userID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(userIDCondition));
            if (!string.IsNullOrEmpty(communityID))
            {
                MiicCondition communityIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.CommunityID),
                       communityID,
                       DbType.String,
                       MiicDBOperatorSetting.Equal);
                condition.Add(new MiicConditionLeaf(communityIDCondition));
            }
            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CommunityMember>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.ID));
            try
            {
                int count = dbService.GetCount<CommunityMember>(column, condition, out message);
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


        bool ICommunityInfo.MemberIgnore(string applyID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(applyID), "参数applyID:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            try
            {
                result = dbService.Update(new CommunityApplicationInfo()
                {
                    ID = applyID,
                    ResponseTime = DateTime.Now,
                    ResponseStatus = ((int)ApplyStatusSetting.Ignore).ToString()
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
            return result;
        }

        bool ICommunityInfo.MemberRefuse(ApproveView approveView)
        {
            Contract.Requires<ArgumentNullException>(approveView != null, "参数approveView:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            MiicConditionCollections condition = approveView.visitor(this);
            try
            {
                result = dbService.UpdateConditions(new CommunityApplicationInfo()
                {
                    ResponseStatus = ((int)ApplyStatusSetting.Refuse).ToString(),
                    ResponseTime = DateTime.Now
                }, condition, out count, out message);
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

        bool ICommunityInfo.MemberAgree(ApproveView approveView)
        {
            Contract.Requires<ArgumentNullException>(approveView != null, "参数approveView:不能为空");
            bool result = false;
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            string message3 = string.Empty;
            MiicConditionCollections conditions = approveView.visitor(this);
            List<string> sqls = new List<string>();
            sqls.Add(DBService.UpdateConditionsSql(new CommunityApplicationInfo()
            {
                ResponseStatus = ((int)ApplyStatusSetting.Agree).ToString(),
                ResponseTime = DateTime.Now
            }, conditions, out message1));
            CommunityMember member = new CommunityMember()
            {
                ID = Guid.NewGuid().ToString(),
                IsAdmin = ((int)MiicYesNoSetting.No).ToString(),
                JoinTime = DateTime.Now,
                CommunityID = approveView.CommunityID,
                MemberID = approveView.MemberID,
                MemberName = approveView.MemberName
            };
            sqls.Add(DBService.InsertSql(member, out message3));
            lock (syncRoot)
            {
                CommunityInfo temp = ((ICommon<CommunityInfo>)this).GetInformation(approveView.CommunityID);
                sqls.Add(DBService.UpdateSql(new CommunityInfo()
                {
                    MemberCount = temp.MemberCount + 1,
                    ID = temp.ID
                }, out message2));
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
                        List<CommunityMember> memberList = new List<CommunityMember>();
                        memberList.Add(member);
                        InsertCaches(memberList);
                        if (items.Find(o => o.ID == approveView.CommunityID) != null)
                        {
                            items.Find(o => o.ID == approveView.CommunityID).MemberCount = temp.MemberCount + 1;
                        }
                    }
                }
            }

            return result;
        }
        List<CommunityMember> ICommunityInfo.GetMemberInfoListByCommunityID(string communityID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(communityID), "参数communityID:不能为空");
            string message = string.Empty;
            List<CommunityMember> result = new List<CommunityMember>();
            try
            {
                result = subitems.FindAll(o => o.CommunityID == communityID);
                if (result.Count == 0)
                {

                    MiicCondition communityIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.CommunityID),
                        communityID,
                        DbType.String,
                        MiicDBOperatorSetting.Equal);
                    MiicConditionSingle condition = new MiicConditionSingle(communityIDCondition);
                    DataTable dt = dbService.GetInformations<CommunityMember>(null, condition, out message);
                    subitems.RemoveAll(o => o.CommunityID == communityID);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (var item in dt.AsEnumerable())
                        {
                            result.Add(new CommunityMember()
                            {
                                ID = item[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.ID)].ToString(),
                                CommunityID = communityID,
                                IsAdmin = item[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.IsAdmin)].ToString(),
                                JoinTime = (DateTime?)item[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, DateTime?>(o => o.JoinTime)],
                                MemberID = item[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.MemberID)].ToString(),
                                MemberName = item[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.MemberName)].ToString(),
                                SortNo = (int?)item[Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, int?>(o => o.SortNo)]
                            });
                        }
                        subitems.AddRange(result);
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

        DataTable ICommunityInfo.GetDetailMemberInfoListByCommunityID(string communityID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(communityID), "参数communityID:不能为空");
            string message = string.Empty;
            DataTable result = new DataTable();
            MiicCondition communityIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.CommunityID),
                communityID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            MiicConditionSingle condition = new MiicConditionSingle(communityIDCondition);

            MiicColumnCollections column = new MiicColumnCollections();
            MiicColumn memberAll = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CommunityMember>());
            column.Add(memberAll);
            MiicColumn userUrl = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<Miic.Friends.User.SimpleUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.User.SimpleUserView, string>(o => o.UserUrl));
            column.Add(userUrl);
            MiicColumn userName = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<Miic.Friends.User.SimpleUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.User.SimpleUserView, string>(o => o.UserName));
            column.Add(userName);
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<CommunityMember>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.MemberID),
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

        DataTable ICommunityInfo.GetPersonAddressBookList(string communityID, string userID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(communityID), "参数communityID:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(userID), "参数userID:不能为空");
            DataTable result = new DataTable();
            string message = string.Empty;
            string sql = "select  " + Config.Attribute.GetSqlTableNameByClassName<AddressBookInfo>() + ".*,AddresserNickName = USER_NAME from  " + Config.Attribute.GetSqlTableNameByClassName<AddressBookInfo>();
            sql += " left join SIMPLE_USER_VIEW ON ADDRESS_BOOK_INFO.ADDRESSER_ID = SIMPLE_USER_VIEW.USER_ID ";
            sql += " where (" + Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<AddressBookInfo, string>(o => o.MyUserID) + " = '" + userID + "') ";
            sql += " and " + Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<AddressBookInfo, string>(o => o.AddresserID) + " not in(";
            sql += "select " + Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<CommunityMember, string>(o => o.MemberID);
            sql += " from  " + Config.Attribute.GetSqlTableNameByClassName<CommunityMember>();
            sql += " where (" + Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<CommunityMember, string>(o => o.CommunityID) + " = '" + communityID + "')";
            sql += ")";
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

        bool ICommunityInfo.IsApplicationHandled(ApproveView approveView)
        {
            Contract.Requires<ArgumentNullException>(approveView != null, "参数approveView:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            MiicConditionCollections condition = approveView.visitor(this);
            try
            {
                count = dbService.GetCount<CommunityApplicationInfo>(new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CommunityApplicationInfo>(),
                    Config.Attribute.GetSqlColumnNameByPropertyName<CommunityApplicationInfo, string>(o => o.ID)), condition, out message);
                if (count == 0)
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

        bool ICommunityInfo.IsApplicationHandledByApplyID(string applyID)
        {
            Contract.Requires<ArgumentNullException>(applyID != null, "参数applyID:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition applicantIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityApplicationInfo, string>(o => o.ID),
                applyID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, applicantIDCondition));
            MiicConditionCollections responseStatusCondition = new MiicConditionCollections();
            MiicCondition applyCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityApplicationInfo, string>(o => o.ResponseStatus),
                ((int)ApplyStatusSetting.Apply).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            responseStatusCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, applyCondition));
            MiicCondition inviteCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityApplicationInfo, string>(o => o.ResponseStatus),
                ((int)ApplyStatusSetting.Invite).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            responseStatusCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.Or, inviteCondition));
            condition.Add(responseStatusCondition);
            try
            {
                count = dbService.GetCount<CommunityApplicationInfo>(new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CommunityApplicationInfo>(),
                    Config.Attribute.GetSqlColumnNameByPropertyName<CommunityApplicationInfo, string>(o => o.ID)), condition, out message);
                if (count == 0)
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

        bool ICommunityInfo.IsMember(string memberID, string communityID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(memberID), "参数memberID:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(communityID), "参数communityID:不能为空");
            bool result = false;
            string message = string.Empty;
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition memberCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.MemberID),
               memberID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, memberCondition));
            MiicCondition communityIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.CommunityID),
                       communityID,
                       DbType.String,
                       MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(communityIDCondition));
            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CommunityMember>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CommunityMember, string>(o => o.ID));
            try
            {
                int count = dbService.GetCount<CommunityMember>(column, condition, out message);
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
    }
}
