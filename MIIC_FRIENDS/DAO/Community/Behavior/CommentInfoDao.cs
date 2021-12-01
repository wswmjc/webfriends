using Miic.Base;
using Miic.DB;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.DB.SqlStruct;
using Miic.Friends.Common;
using Miic.Friends.Community.Behavior;
using Miic.Friends.Notice;
using Miic.Log;
using Miic.Manage.User;
using Miic.Manage.User.Setting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Community.Behavior
{
    public class CommentInfoDao : NoRelationCommon<CommentInfo>, ICommunityBehavior<CommentInfo>
    {
        private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
        private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
        private object syncRoot = new object();
        public static bool DeleteCacheByPublishID(string publishID)
        {
            bool result = false;
            if (items.Count > 0)
            {
                try
                {
                    items.RemoveAll(o => o.PublishID == publishID);
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

            }
            else
            {
                result = true;
            }
            return result;
        }

        bool ICommon<CommentInfo>.Insert(CommentInfo commentInfo)
        {
            Contract.Requires<ArgumentNullException>(commentInfo != null, "参数commentInfo：不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(commentInfo.ID), "参数commentInfo.ID：不能为空！");
            bool result = false;
            List<string> sqls = new List<string>();
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            string message3 = string.Empty;
            IPublishInfo Itemp = new PublishInfoDao();
            sqls.Add(DBService.InsertSql(commentInfo, out message1));
            try
            {
                PublishInfo temp = ((ICommon<PublishInfo>)Itemp).GetInformation(commentInfo.PublishID);
                sqls.Add(DBService.UpdateSql(new PublishInfo()
                {
                    ID = commentInfo.PublishID,
                    CommentNum = temp.CommentNum + 1
                }, out message2));

                NoticeInfo noticeInfo = new NoticeInfo()
                {
                    ID = Guid.NewGuid().ToString(),
                    NoticerID = commentInfo.ToCommenterID,
                    NoticerName = commentInfo.ToCommenterName,
                    Source = ((int)Miic.Friends.Common.Setting.BusinessTypeSetting.Community).ToString(),
                    NoticeType = ((int)Miic.Friends.Notice.Setting.NoticeTypeSetting.Message).ToString(),
                    PublisherID = commentInfo.FromCommenterID,
                    PublisherName = commentInfo.FromCommenterName,
                    PublishID = commentInfo.PublishID,
                    PublishTime = commentInfo.CommentTime,
                    ReadStatus = ((int)Miic.Base.Setting.MiicReadStatusSetting.UnRead).ToString(),
                    CommentID = commentInfo.ID
                };

                if (noticeInfo.NoticerID != noticeInfo.PublisherID)
                {
                    sqls.Add(DBService.InsertSql(noticeInfo, out message3));
                }
                //积分
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = commentInfo.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Comment).ToString(),
                    Score = ScoreConfig.Score.CommentScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = commentInfo.FromCommenterID,
                    UserName = commentInfo.FromCommenterName
                }, out message));
                result = dbService.excuteSqls(sqls, out message);
                if (result == true)
                {
                    InsertCache(commentInfo);
                    lock (syncRoot)
                    {
                        if (PublishInfoDao.items.Find(o => o.ID == commentInfo.PublishID) != null)
                        {
                            PublishInfoDao.items.Find(o => o.ID == commentInfo.PublishID).CommentNum = temp.CommentNum + 1;
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

        bool ICommon<CommentInfo>.Update(CommentInfo commentInfo)
        {
            Contract.Requires<ArgumentNullException>(commentInfo != null, "参数commentInfo:不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(commentInfo.ID), "参数commentInfo.ID:不能为空，因为是主键");
            int count = 0;
            string message = string.Empty;
            bool result = false;
            try
            {
                result = dbService.Update(commentInfo, out count, out message);
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
                DeleteCache(o => o.ID == commentInfo.ID);
            }
            return result;
        }

        bool ICommon<CommentInfo>.Delete(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            bool result = false;
            List<string> sqls = new List<string>();
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            IPublishInfo Itemp = new PublishInfoDao();
            try
            {
                CommentInfo tempCommentInfo = ((ICommon<CommentInfo>)this).GetInformation(id);
                PublishInfo tempPublishInfo = ((ICommon<PublishInfo>)Itemp).GetInformation(tempCommentInfo.PublishID);
                sqls.Add(DBService.DeleteSql(new CommentInfo()
                {
                    ID = id
                }, out message1));
                sqls.Add(DBService.UpdateSql(new PublishInfo()
                {
                    ID = tempCommentInfo.PublishID,
                    CommentNum = tempPublishInfo.CommentNum - 1
                }, out message2));
                //删除级联noticeInfo
                MiicConditionCollections noticeCondition = new MiicConditionCollections(MiicDBLogicSetting.No);
                MiicCondition commentIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<NoticeInfo, string>(o => o.CommentID),
                    id,
                    DbType.String,
                    MiicDBOperatorSetting.Equal);
                noticeCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, commentIDCondition));
                MiicCondition readStatusCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<NoticeInfo, string>(o => o.ReadStatus),
               ((int)Miic.Base.Setting.MiicReadStatusSetting.UnRead).ToString(),
               DbType.String,
               MiicDBOperatorSetting.Equal);
                noticeCondition.Add(new MiicConditionLeaf(readStatusCondition));
                sqls.Add(DBService.DeleteConditionsSql<NoticeInfo>(noticeCondition, out message));
                //积分
                sqls.Add(DBService.InsertSql<UserScopeHistory>(new UserScopeHistory()
                {
                    ID = Guid.NewGuid().ToString(),
                    BusinessID = tempCommentInfo.ID,
                    CreateTime = DateTime.Now,
                    GetWay = ((int)GetWayTypeSetting.Comment).ToString(),
                    Score = -1 * ScoreConfig.Score.CommentScore,
                    ServiceID = ScoreConfig.ServiceID,
                    UserID = tempCommentInfo.FromCommenterID,
                    UserName = tempCommentInfo.FromCommenterName
                }, out message));
                result = dbService.excuteSqls(sqls, out message);
                if (result == true)
                {
                    DeleteCache(o => o.ID == id);
                    lock (syncRoot)
                    {
                        if (PublishInfoDao.items.Find(o => o.ID == tempCommentInfo.PublishID) != null)
                        {
                            PublishInfoDao.items.Find(o => o.ID == tempCommentInfo.PublishID).CommentNum = tempPublishInfo.CommentNum - 1;
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

        CommentInfo ICommon<CommentInfo>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            CommentInfo result = null;
            string message = string.Empty;
            try
            {
                result = items.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new CommentInfo
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
                    result = Config.Serializer.Deserialize<CommentInfo>(serializer);
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
        /// 获取行业圈子信息的所有评论
        /// </summary>
        /// <param name="publishID">行业圈子信息ID</param>
        /// <param name="page">分页</param>
        /// <returns>行业圈子信息评论列表</returns>
        public DataTable GetCommentList(string publishID, MiicPage page)
        {
            DataTable result = new DataTable();
            MiicColumnCollections column = new MiicColumnCollections();
            List<MiicOrderBy> orders = new List<MiicOrderBy>();
            string message = string.Empty;
            string sql = string.Empty;
            Cookie cookie = new Cookie();
            string UserID = cookie.GetCookie("SNS_ID", out message);
            if (string.IsNullOrEmpty(UserID))
            {
                throw new Miic.MiicException.MiicCookieArgumentNullException("UserID不能为空，Cookie失效");
            }
            sql += "SELECT * FROM GetCommunityCommentListWithAddress('" + UserID + "','" + publishID + "') ";
            if (page != null)
            {
                sql = "WITH INFO_PAGE AS ( SELECT row_number() OVER ( ORDER BY  Temp.COMMENT_TIME DESC) as row,Temp.*  FROM ( " + sql;
                sql += " ) as Temp) ";
                sql += "SELECT * FROM INFO_PAGE where row between " + page.pageStart + " and " + page.pageEnd + ";";
            }
            else
            {
                sql += " ORDER BY COMMENT_TIME DESC";
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

        /// <summary>
        /// 获取行业圈子信息评论总数
        /// </summary>
        /// <param name="publishID">行业圈子信息ID</param>
        /// <returns>评论总数</returns>
        public int GetCommentCount(string publishID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(publishID), "参数publishID:不能为空");
            int result = 0;
            string message = string.Empty;
            MiicCondition infoCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<CommentInfo, string>(o => o.PublishID),
                                                          publishID,
                                                          DbType.String,
                                                          MiicDBOperatorSetting.Equal);
            MiicConditionSingle condition = new MiicConditionSingle(infoCondition);
            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<CommentInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<CommentInfo, string>(o => o.ID));
            try
            {
                result = dbService.GetCount<CommentInfo>(column, condition, out message);
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
