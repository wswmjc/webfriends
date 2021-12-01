using Miic.Base;
using Miic.Base.Setting;
using Miic.Common;
using Miic.DB;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Miic.Friends.Community
{
    public partial class LabelInfoDao : NoRelationCommon<LabelInfo>, ILabelInfo
    {
        private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
        private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
        public LabelInfoDao() { }
        /// <summary>
        /// 根据行业圈子ID删除标签缓存
        /// </summary>
        /// <param name="communityID">行业圈子ID</param>
        /// <returns>Yes/No</returns>
        public static bool DeleteCacheByCommunityID(string communityID)
        {
            bool result = false;
            try
            {
                List<LabelInfo> cacheLabelList = LabelInfoDao.items.FindAll(o => o.CommunityID == communityID);
                foreach (var item in cacheLabelList.AsEnumerable())
                {
                    DeleteCache(o => o.ID == item.ID);
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
        bool ICommon<LabelInfo>.Insert(LabelInfo labelInfo)
        {
            Contract.Requires<ArgumentNullException>(labelInfo != null, "参数labelInfo：不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(labelInfo.ID), "参数labelInfo.ID：不能为空！");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            labelInfo.CreateTime = DateTime.Now;
            labelInfo.Valid = ((int)MiicValidTypeSetting.Valid).ToString();
            try
            {
                result = dbService.Insert(labelInfo, out count, out message);
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
                InsertCache(labelInfo);
            }
            return result;
        }

        bool ICommon<LabelInfo>.Update(LabelInfo labelInfo)
        {
            Contract.Requires<ArgumentNullException>(labelInfo != null, "参数labelInfo:不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(labelInfo.ID), "参数LabelInfo.ID:不能为空，因为是主键");
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            bool result = false;
            List<string> sqls = new List<string>();
            sqls.Add(DBService.UpdateSql(labelInfo, out message1));
            MiicCondition labelIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelID),
                labelInfo.ID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            MiicConditionSingle condition = new MiicConditionSingle(labelIDCondition);
            sqls.Add(DBService.UpdateConditionSql<PublishLabelRelation>(new PublishLabelRelation()
            {
                LabelName = labelInfo.LabelName
            }, condition, out message2));
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
                DeleteCache(o => o.ID == labelInfo.ID);
            }

            return result;
        }

        bool ICommon<LabelInfo>.Delete(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            bool result = false;
            string message = string.Empty;
            bool isUsed = IsUsed(id);
            if (isUsed == false) 
            {
                int count = 0;
                try
                {
                    result = dbService.Delete(new LabelInfo() { ID = id }, out count, out message);
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
                string message1 = string.Empty;
                List<string> sqls = new List<string>();
                sqls.Add(DBService.UpdateSql<LabelInfo>(new LabelInfo()
                {
                    ID = id,
                    Valid = ((int)MiicValidTypeSetting.InValid).ToString(),
                    EndTime = DateTime.Now
                }, out message1));
                MiicCondition labelIDCondition=new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation,string>(o=>o.LabelID),
                    id,
                    DbType.String,
                    MiicDBOperatorSetting.Equal);
                MiicConditionSingle condition=new MiicConditionSingle(labelIDCondition);
                sqls.Add(DBService.UpdateConditionSql(new PublishLabelRelation(){
                 Valid = ((int)MiicValidTypeSetting.InValid).ToString()
               },condition,out message1));
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
                DeleteCache(o => o.ID == id);
            }
            return result;
        }
        /// <summary>
        /// 标签是否被使用
        /// </summary>
        /// <param name="labelID">标签ID</param>
        /// <returns>Yes/No</returns>
        private bool IsUsed(string labelID) 
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(labelID), "参数labelID:不能为空");
            bool result = false;
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
            int count = 0;
            try
            {
                count = dbService.GetCount<PublishLabelRelation>(null, condition, out message);
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
            if (count > 0)
            {
                result = true;
            }
            return result;
        }
        LabelInfo ICommon<LabelInfo>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            LabelInfo result = null;
            string message = string.Empty;
            try
            {
                result = items.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new LabelInfo
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
                    result = Config.Serializer.Deserialize<LabelInfo>(serializer);
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
        /// 设置标签的有效性
        /// </summary>
        /// <param name="validView">有效性视图</param>
        /// <returns>Yes/No</returns>
        public bool SetValid(ValidView validView)
        {
            bool result = false;
            int count = 0;
            string message = string.Empty;
            try
            {
                result = dbService.Update<LabelInfo>(new LabelInfo()
                {
                    ID = validView.ID,
                    Valid = ((int)validView.Valid).ToString()
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
                DeleteCache(o => o.ID == validView.ID);
            }
            return result;
        }


        /// <summary>
        /// 根据圈子获取有效标签集合
        /// </summary>
        /// <param name="communityID">圈子ID</param>
        /// <returns>标签集合</returns>
        public DataTable GetLabelInfosByCommunityID(string communityID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(communityID), "参数communityID：不能为空！");
            string message = string.Empty;
            DataTable result = new DataTable();
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition communityIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.CommunityID),
                communityID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, communityIDCondition));
            MiicCondition validCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.Valid),
                ((int)MiicValidTypeSetting.Valid).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(validCondition));
            List<MiicOrderBy> orders = new List<MiicOrderBy>();
            orders.Add(new MiicOrderBy()
            {
                Desc = false,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, int?>(o => o.SortNo)
            });
            condition.order = orders;
            try
            {
                result = dbService.GetInformations<LabelInfo>(null, condition, out message);
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
        /// 检测标签名称的唯一性
        /// </summary>
        /// <param name="communityID">圈子ID</param>
        /// <param name="labelName">标签名称</param>
        /// <returns>不存在true 存在false</returns>
        public bool UniqueLabel(string communityID, string labelName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(communityID), "参数communityID：不能为空！");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(labelName), "参数labelName：不能为空！");
            bool result = false;
            string message = string.Empty;
            MiicConditionCollections condition = new MiicConditionCollections();
            MiicCondition communityIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.CommunityID),
                communityID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, communityIDCondition));
            MiicCondition labelNameCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.LabelName),
                labelName.Trim(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(labelNameCondition));
            MiicColumn labelIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.ID));
            try
            {
                int count = dbService.GetCount<LabelInfo>(labelIDColumn, condition, out message);
                if (count == 0)
                {
                    result = true;
                }
                else
                {
                    result = false;
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
        /// 检测标签是否被使用
        /// </summary>
        /// <param name="labelID">标签ID</param>
        /// <returns>使用true 未使用false</returns>
        public bool HasUsed(string labelID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(labelID), "参数labelID：不能为空！");
            bool result = false;
            string message = string.Empty;
            MiicCondition labelIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelID),
                labelID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            MiicColumn labelIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<PublishLabelRelation>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelID));
            try
            {
                int count = dbService.GetCount<PublishLabelRelation>(labelIDColumn, new MiicConditionSingle(labelIDCondition), out message);
                if (count == 0)
                {
                    result = false;
                }
                else
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
        /// 批量禁用标签
        /// </summary>
        /// <param name="labelIDs">标签ID组</param>
        /// <returns>yes/no</returns>
        public bool DisableLabels(List<string> labelIDs)
        {
            Contract.Requires<ArgumentNullException>(labelIDs != null && labelIDs.Count != 0, "参数labelIDs：不能为空！");
            bool result = false;
            string message = string.Empty;
            string message1 = string.Empty;
            List<string> sqls = new List<string>();
            foreach (var item in labelIDs)
            {
                MiicCondition labelIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.ID),
                   item,
                   DbType.String,
                   MiicDBOperatorSetting.Equal);
                MiicConditionSingle condition = new MiicConditionSingle(labelIDCondition);
                sqls.Add(DBService.UpdateConditionSql<LabelInfo>(new LabelInfo()
                {
                    Valid = ((int)MiicValidTypeSetting.InValid).ToString()
                }, condition, out message1));
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
                foreach (var item in labelIDs)
                {
                    DeleteCache(o => o.ID == item);
                }
            }
            return result;
        }

        /// <summary>
        /// 删除Label信息(管理员使用)
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>Yes/No</returns>
        /// <remarks>包括Relation的内容，级联</remarks>
        public bool DeepDelete(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id：不能为空！");
            bool result = false;
            string message = string.Empty,
                message1 = string.Empty,
                message2 = string.Empty;
            List<string> sqls = new List<string>();
            MiicCondition labelIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<PublishLabelRelation, string>(o => o.LabelID),
                id,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            MiicConditionSingle condition = new MiicConditionSingle(labelIDCondition);
            sqls.Add(DBService.DeleteConditionSql<PublishLabelRelation>(condition, out message1));
            sqls.Add(DBService.DeleteSql(new LabelInfo() { ID = id }, out message2));
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
                DeleteCache(o => o.ID == id);
            }
            return result;
        }

        /// <summary>
        /// 根据标签ID获取指定标签集合
        /// </summary>
        /// <param name="labelIDs">标签ID数组</param>
        /// <returns>标签集合</returns>
        DataTable ILabelInfo.GetLabelListWithIDs(List<string> labelIDs)
        {
            Contract.Requires<ArgumentNullException>(labelIDs != null, "参数labelIDs:不为空！");
            Contract.Requires<ArgumentOutOfRangeException>(labelIDs.Count != 0, "参数labelIDs:不为空！");
            DataTable result = new DataTable();
            string message = string.Empty;
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition validCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<LabelInfo, string>(o => o.Valid),
             ((int)MiicValidTypeSetting.Valid).ToString(),
              DbType.String,
              MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, validCondition));

            MiicConditionCollections idsCondition = new MiicConditionCollections();
            for (int i = 0, lableCount = labelIDs.Count; i < lableCount; i++)
            {
                if (i == 0)
                {
                    idsCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<LabelInfo, string>(o => o.ID),
                        labelIDs[i],
                        DbType.String,
                        MiicDBOperatorSetting.Equal)));
                }
                else
                {
                    idsCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.Or, new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<LabelInfo, string>(o => o.ID),
                        labelIDs[i],
                        DbType.String,
                        MiicDBOperatorSetting.Equal)));
                }
            }
            condition.Add(idsCondition);
            List<MiicOrderBy> order = new List<MiicOrderBy>();
            order.Add(new MiicOrderBy()
            {
                Desc = true,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<LabelInfo, int?>(o => o.SortNo)
            });
            condition.order = order;
           
            MiicRelationCollections realtions = new MiicRelationCollections(Config.Attribute.GetSqlTableNameByClassName<LabelInfo>());
            MiicFriendRelation searchInfoRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.ID),
                Config.Attribute.GetSqlTableNameByClassName<LabelSearchInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.LabelID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.LeftJoin);
            realtions.Add(searchInfoRelation);
            MiicFriendRelation userInfoRelation = new MiicFriendRelation(Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.CreaterID),
                Config.Attribute.GetSqlTableNameByClassName<Miic.Friends.User.SimpleUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.User.SimpleUserView, string>(o => o.UserID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.LeftJoin);
            realtions.Add(userInfoRelation);


            MiicColumnCollections columns = new MiicColumnCollections();
            MiicColumn labelIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.ID));
            columns.Add(labelIDColumn);
            MiicColumn labelNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.LabelName));
            columns.Add(labelNameColumn);
            MiicColumn createTimeColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, DateTime?>(o => o.CreateTime));
            columns.Add(createTimeColumn);
            MiicColumn createrIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.CreaterID));
            columns.Add(createrIDColumn);
            MiicColumn createrNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<Miic.Friends.User.SimpleUserView>(),
                "",
                Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.User.SimpleUserView, string>(o => o.UserName)
                , Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.CreaterName));
            columns.Add(createrNameColumn);

            MiicColumn labelSearchCommunityIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelSearchInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.CommunityID));
            columns.Add(labelSearchCommunityIDColumn);
            MiicColumn labelSearchPublishIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelSearchInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.PublishID));
            columns.Add(labelSearchPublishIDColumn);
            MiicColumn labelSearchUserIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelSearchInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.UserID));
            columns.Add(labelSearchUserIDColumn);
            MiicColumn labelSearchUserNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelSearchInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.UserName));
            columns.Add(labelSearchUserNameColumn);
            MiicColumn labelSearchUserTypeColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelSearchInfo>(),
              Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.UserType));
            columns.Add(labelSearchUserTypeColumn);
            MiicColumn labelSearchUserUrlColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelSearchInfo>(),
             Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.UserUrl));
            columns.Add(labelSearchUserUrlColumn);
            try
            {
               
                result = dbService.GetInformations(columns, realtions, condition, out message);
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

        DataTable ILabelInfo.Search(NoPersonKeywordView keywordView, MiicPage page)
        {
            Contract.Requires<ArgumentNullException>(keywordView != null, "参数keywordView:不为空！");
            DataTable result = new DataTable();
            string message = string.Empty;
            MiicConditionCollections condition = keywordView.visitor(this);
           
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<LabelInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.ID),
                Config.Attribute.GetSqlTableNameByClassName<LabelSearchInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.LabelID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.LeftJoin);
            MiicColumnCollections columns = new MiicColumnCollections();
            MiicColumn labelIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.ID));
            columns.Add(labelIDColumn);
            MiicColumn labelNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.LabelName));
            columns.Add(labelNameColumn);
           
            MiicColumn createTimeColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, DateTime?>(o => o.CreateTime));
            columns.Add(createTimeColumn);
            MiicColumn createrIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.CreaterID));
            columns.Add(createrIDColumn);
            MiicColumn createrNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.CreaterName));
            columns.Add(createrNameColumn);
            MiicColumn sortNoColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, int?>(o => o.SortNo));
            columns.Add(sortNoColumn);

            MiicColumn labelSearchPublishIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelSearchInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.PublishID));
            columns.Add(labelSearchPublishIDColumn);
            MiicColumn labelSearchUserIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelSearchInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.UserID));
            columns.Add(labelSearchUserIDColumn);
            MiicColumn labelSearchUserNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelSearchInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.UserName));
            columns.Add(labelSearchUserNameColumn);
            MiicColumn labelSearchUserTypeColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelSearchInfo>(),
              Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.UserType));
            columns.Add(labelSearchUserTypeColumn);
            MiicColumn labelSearchUserUrlColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<LabelSearchInfo>(),
             Config.Attribute.GetSqlColumnNameByPropertyName<LabelSearchInfo, string>(o => o.UserUrl));
            columns.Add(labelSearchUserUrlColumn);
            try
            {
                if (page != null)
                {
                    List<MiicOrderBy> order = new List<MiicOrderBy>();
                    order.Add(new MiicOrderBy()
                    {
                         Desc=true,
                         PropertyName=Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<LabelInfo,int?>(o=>o.SortNo)
                    });
                    condition.order = order;
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

        int ILabelInfo.GetSearchCount(NoPersonKeywordView keywordView)
        {
            Contract.Requires<ArgumentNullException>(keywordView != null, "参数keywordView:不为空！");
            int result = 0;
            string message = string.Empty;
            MiicConditionCollections condition = keywordView.visitor(this);
            MiicCondition validCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<LabelInfo, string>(o => o.Valid),
                ((int)MiicValidTypeSetting.Valid).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(validCondition));
            try
            {
                result = dbService.GetCount<LabelInfo>(null, condition, out message);
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
