using Miic.Base;
using Miic.DB;
using Miic.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Miic.Base.Setting;
using System.Data;
using Miic.DB.SqlObject;
using Miic.DB.Setting;
using Miic.Friends.General.SimpleGroup;
using Miic.Manage.User;

namespace Miic.Friends.AddressBook
{
    public partial class AddressBookInfoDao : RelationCommon<AddressBookInfo, AddressBookApplicationInfo>, IAddressBookInfo
    {
        private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
        private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
        bool ICommon<AddressBookInfo>.Insert(AddressBookInfo addressBookInfo)
        {
            Contract.Requires<ArgumentNullException>(addressBookInfo!=null, "参数addressBookInfo:不能为空");
            bool result = false;
            string message1 = string.Empty;
            string message2=string.Empty;
            string message=string.Empty;
            List<string>sqls=new List<string>();
            sqls.Add(DBService.UpdateSql<AddressBookApplicationInfo>(new AddressBookApplicationInfo()
            {
                ID = addressBookInfo.ID,
                ResponseStatus = ((int)MiicSimpleApproveStatusSetting.Agree).ToString()
            }, out message1));
            sqls.Add(DBService.InsertSql(addressBookInfo, out message2));
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
                DeleteCache(o => o.ID == addressBookInfo.ID);
                InsertCache(addressBookInfo);
            }
            return result;
        }

        bool ICommon<AddressBookInfo>.Update(AddressBookInfo addressBookInfo)
        {
            Contract.Requires<ArgumentNullException>(addressBookInfo != null, "参数addressBookInfo:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            try
            {
                result = dbService.Update(addressBookInfo, out count, out message);
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
                DeleteCache(false,o=>o.ID==addressBookInfo.ID);
            }
            return result;
        }

        bool ICommon<AddressBookInfo>.Delete(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            bool result = false;
            string message = string.Empty;
            int count = 0;
            try
            {
                result = dbService.Delete<AddressBookInfo>(new AddressBookInfo()
                {
                    ID = id
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
                DeleteCache(false, o => o.ID == id);
            }
            return result;
        }

        AddressBookInfo ICommon<AddressBookInfo>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            AddressBookInfo result = null;
            string message = string.Empty;
            try
            {
                result = items.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new AddressBookInfo
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
                    result = Config.Serializer.Deserialize<AddressBookInfo>(serializer);
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

        public bool Insert(string ID) 
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(ID), "参数ID:不能为空");
            bool result=false;
            AddressBookApplicationInfo application = ((ICommon<AddressBookApplicationInfo>)this).GetInformation(ID);
            if (application != null) 
            {
                AddressBookInfo addressBookInfo = new AddressBookInfo()
                {
                    ID = application.ID,
                    MyUserID = application.MyUserID,
                    AddresserID = application.AddresserID,
                    AddresserName = application.AddresserName,
                    IsBlackList = ((int)MiicYesNoSetting.No).ToString(),
                    CanSeeAddresser = ((int)MiicYesNoSetting.Yes).ToString(),
                    CanSeeAddresserTime = DateTime.Now,
                    CanSeeMe = ((int)MiicYesNoSetting.Yes).ToString(),
                    CanSeeMeTime = DateTime.Now,
                    ApplicationTime = application.ApplicationTime,
                    OftenUsed=((int)MiicYesNoSetting.No).ToString(),
                    JoinTime = DateTime.Now
                };
               result = ((ICommon<AddressBookInfo>)this).Insert(addressBookInfo);
            }
            return result;
        }


        public bool SetRemark(SetRemarkView remarkView)
        {
            Contract.Requires<ArgumentNullException>(remarkView!=null, "参数remarkView:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            try
            {
                result = dbService.Update(new AddressBookInfo() 
                {
                    ID=remarkView.ID,
                    Remark=remarkView.Remark
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
                DeleteCache(false, o => o.ID == remarkView.ID);
            }
            return result;
        }

        public bool SetCanSeeMe(SetCanSeeMeView canSeeMeView)
        {
            Contract.Requires<ArgumentNullException>(canSeeMeView != null, "参数canSeeMeView:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            try
            {
                result = dbService.Update(new AddressBookInfo() 
                {
                    ID=canSeeMeView.ID,
                    CanSeeMe=canSeeMeView.CanSeeMe==true?((int)MiicYesNoSetting.Yes).ToString():((int)MiicYesNoSetting.No).ToString(),
                    CanSeeMeTime=DateTime.Now
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
                DeleteCache(false, o => o.ID ==canSeeMeView.ID);
            }
            return result;
        }

        public bool SetCanSeeAddresser(SetCanSeeAddresserView canSeeAddresserView)
        {
            Contract.Requires<ArgumentNullException>(canSeeAddresserView != null, "参数canSeeAddresserView:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            try
            {
                result = dbService.Update(new AddressBookInfo()
                {
                    ID = canSeeAddresserView.ID,
                    CanSeeAddresser = canSeeAddresserView.CanSeeAddresser == true ? ((int)MiicYesNoSetting.Yes).ToString() : ((int)MiicYesNoSetting.No).ToString(),
                    CanSeeAddresserTime= DateTime.Now
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
                DeleteCache(false, o => o.ID == canSeeAddresserView.ID);
            }
            return result;
        }
        public bool SetOftenUsed(SetOftenUsedView oftenUsedView) 
        {
            Contract.Requires<ArgumentNullException>(oftenUsedView != null, "参数oftenUsedView:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            try
            {
                result = dbService.Update(new AddressBookInfo()
                {
                    ID = oftenUsedView.ID,
                    OftenUsed =oftenUsedView.OftenUsed== true ? ((int)MiicYesNoSetting.Yes).ToString() : ((int)MiicYesNoSetting.No).ToString()
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
                DeleteCache(false, o => o.ID == oftenUsedView.ID);
            }
            return result;
        }
        public DataTable GetOffenUsedAddressBookList(string userID, int top)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(userID), "参数userID:不能为空");
            DataTable result = new DataTable();
            string message = string.Empty;
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<AddressBookInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserID),
                Config.Attribute.GetSqlTableNameByClassName<SimplePersonUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView, string>(o => o.UserID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.InnerJoin);
            MiicConditionCollections conditions = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition userIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.MyUserID),
                userID,
                DbType.String,
               MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(MiicDBLogicSetting.No,userIDCondition));
            MiicCondition oftenUsedCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.OftenUsed),
                ((int)MiicYesNoSetting.Yes).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(oftenUsedCondition));
            MiicColumnCollections columns=new MiicColumnCollections(new MiicTop(top));
            MiicColumn addresserIDColumn=new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AddressBookInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo,string>(o=>o.AddresserID));
            columns.Add(addresserIDColumn);
            MiicColumn addresserNameColumn=new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AddressBookInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo,string>(o=>o.AddresserName));
            columns.Add(addresserNameColumn);
            MiicColumn remarkColumn=new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AddressBookInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo,string>(o=>o.Remark));
            columns.Add(remarkColumn);
            MiicColumn addresserUrlColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<SimplePersonUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView, string>(o => o.UserUrl));
            columns.Add(addresserUrlColumn);
            MiicColumn oftenUsedColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AddressBookInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.OftenUsed));
            columns.Add(oftenUsedColumn);
            try
            {
                result = dbService.GetInformations(columns, relation,conditions, out message);
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
            MiicCondition isBlackListCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.IsBlackList),
              ((int)MiicYesNoSetting.No).ToString(),
              DbType.String,
              MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(isBlackListCondition));
            List<MiicOrderBy> order = new List<MiicOrderBy>();
            order.Add(new MiicOrderBy()
            {
                 Desc=true,
                 PropertyName=Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo,string>(o=>o.AddresserName)
            });
            order.Add(new MiicOrderBy()
            {
                Desc = true,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.OftenUsed)
            });
            conditions.order = order;
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<AddressBookInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserID),
                Config.Attribute.GetSqlTableNameByClassName<Miic.Friends.User.SimpleUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.User.SimpleUserView, string>(o => o.UserID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.InnerJoin);
            MiicColumnCollections columns = new MiicColumnCollections();
            MiicColumn addressBookInfoColumns = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AddressBookInfo>());
            columns.Add(addressBookInfoColumns);
            MiicColumn ImageUrlColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<Miic.Friends.User.SimpleUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.User.SimpleUserView, string>(o => o.UserUrl));
            columns.Add(ImageUrlColumn);
            MiicColumn NickNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<Miic.Friends.User.SimpleUserView>(),
                "",
               Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.User.SimpleUserView, string>(o => o.UserName),"NickName");
            columns.Add(NickNameColumn);
            if (page != null)
            {
                result = dbService.GetInformationsPage(columns, relation, conditions, page, out message);
            }
            else 
            {
                result = dbService.GetInformations(columns, relation, conditions, out message);
            }
            return result;
        }

        public int GetSearchCount(GeneralSimpleGroupSearchView searchView)
        {
            Contract.Requires<ArgumentNullException>(searchView != null, "参数searchView:不能为空");
            int result = 0;
            MiicConditionCollections conditions = searchView.visitor(this);
            MiicCondition isBlackListCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.IsBlackList),
             ((int)MiicYesNoSetting.No).ToString(),
             DbType.String,
             MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(isBlackListCondition));
            string message = string.Empty;
            try
            {
                result = dbService.GetCount<AddressBookInfo>(null,conditions, out message);
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


        public DataTable GetPersonBlackListInfos(GeneralSimpleGroupSearchView searchView, MiicPage page = null)
        {
            Contract.Requires<ArgumentNullException>(searchView != null, "参数searchView:不能为空");
            DataTable result = new DataTable();
            string message = string.Empty;
            MiicConditionCollections conditions = searchView.visitor(this);
            MiicCondition isBlackListCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.IsBlackList),
                ((int)MiicYesNoSetting.Yes).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(isBlackListCondition));
            List<MiicOrderBy> order = new List<MiicOrderBy>();
            order.Add(new MiicOrderBy()
            {
                Desc = true,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserName)
            });
            order.Add(new MiicOrderBy()
            {
                Desc = true,
                PropertyName = Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.OftenUsed)
            });
            conditions.order = order;
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<AddressBookInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserID),
                Config.Attribute.GetSqlTableNameByClassName<MiicSocialUserInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<MiicSocialUserInfo, string>(o => o.ID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.InnerJoin);
            MiicColumnCollections columns = new MiicColumnCollections();
            MiicColumn addressBookInfoColumns = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AddressBookInfo>());
            columns.Add(addressBookInfoColumns);
            MiicColumn ImageUrlColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<MiicSocialUserInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<MiicSocialUserInfo, string>(o => o.MicroUserUrl));
            columns.Add(ImageUrlColumn);
            if (page != null)
            {
                result = dbService.GetInformationsPage(columns, relation, conditions, page, out message);
            }
            else
            {
                result = dbService.GetInformations(columns, relation, conditions, out message);
            }
            return result;
        }

        public int GetPersonBlackListCount(GeneralSimpleGroupSearchView searchView)
        {
            Contract.Requires<ArgumentNullException>(searchView != null, "参数searchView:不能为空");
            int result = 0;
            MiicConditionCollections conditions = searchView.visitor(this);
            MiicCondition isBlackListCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.IsBlackList),
               ((int)MiicYesNoSetting.Yes).ToString(),
               DbType.String,
               MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(isBlackListCondition));
            string message = string.Empty;
            try
            {
                result = dbService.GetCount<AddressBookInfo>(null, conditions, out message);
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
        bool IAddressBookInfo.Remove(string firestUserID, string secondUserID)
        {
            bool result = false;
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            List<string> sqls = new List<string>();
            MiicConditionCollections firstCondition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition firstMyUserIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.MyUserID),
                firestUserID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            firstCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, firstMyUserIDCondition));
            MiicCondition firstAddresserIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserID),
                secondUserID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            firstCondition.Add(new MiicConditionLeaf(firstAddresserIDCondition));
            sqls.Add(DBService.DeleteConditionsSql<AddressBookInfo>(firstCondition,out message1));
            MiicConditionCollections secondCondition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition secondMyUserIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.MyUserID),
              secondUserID,
              DbType.String,
              MiicDBOperatorSetting.Equal);
            secondCondition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, secondMyUserIDCondition));
            MiicCondition secondAddresserIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserID),
                firestUserID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            secondCondition.Add(new MiicConditionLeaf(secondAddresserIDCondition));
            sqls.Add(DBService.DeleteConditionsSql<AddressBookInfo>(secondCondition, out message2));
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
                items.RemoveAll(o => (o.AddresserID == firestUserID && o.MyUserID == secondUserID) || (o.AddresserID == secondUserID && o.MyUserID == firestUserID));
            }
            return result;
        }

        /// <summary>
        /// 是否联系人对自己可见
        /// </summary>
        /// <param name="myID"></param>
        /// <param name="addresserID"></param>
        /// <returns></returns>
        public bool CanSeeAddresser(string myID, string addresserID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(myID), "参数myID:不能为空");
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(addresserID), "参数addresserID:不能为空");
            bool result = false;
            string message = string.Empty;
            MiicConditionCollections condition = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition myIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.MyUserID),
                addresserID,
                 DbType.String,
                  MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, myIDCondition));
            MiicCondition addresserIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserID),
               myID,
                DbType.String,
                 MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(addresserIDCondition));
            MiicCondition canSeeAddresserCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.CanSeeMe),
               ((int)MiicYesNoSetting.Yes).ToString(),
                DbType.String,
                 MiicDBOperatorSetting.Equal);
            condition.Add(new MiicConditionLeaf(canSeeAddresserCondition));

            MiicColumn idColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AddressBookInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.ID));

            try
            {
                int addresser = dbService.GetCount<AddressBookInfo>(idColumn, condition, out message);
                if (addresser == 1)
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
 
        DataTable IAddressBookInfo.GetAllOftenUsedAddressBookList(string userID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(userID), "参数userID:不能为空");
            DataTable result = new DataTable();
            string message = string.Empty;
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<AddressBookInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserID),
                Config.Attribute.GetSqlTableNameByClassName<SimplePersonUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView, string>(o => o.UserID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.InnerJoin);
            MiicConditionCollections conditions = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition userIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.MyUserID),
                userID,
                DbType.String,
               MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, userIDCondition));
            MiicCondition oftenUsedCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.OftenUsed),
                ((int)MiicYesNoSetting.Yes).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(oftenUsedCondition));
            MiicColumnCollections columns = new MiicColumnCollections();
            MiicColumn addresserIDColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AddressBookInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserID));
            columns.Add(addresserIDColumn);
            MiicColumn addresserNameColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AddressBookInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserName));
            columns.Add(addresserNameColumn);
            MiicColumn remarkColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AddressBookInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.Remark));
            columns.Add(remarkColumn);
            MiicColumn addresserUrlColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<SimplePersonUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView, string>(o => o.UserUrl));
            columns.Add(addresserUrlColumn);
            MiicColumn oftenUsedColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AddressBookInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.OftenUsed));
            columns.Add(oftenUsedColumn);
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
        
    }
}
