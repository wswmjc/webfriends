using Miic.Base;
using Miic.Base.Setting;
using Miic.DB;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.AddressBook.Setting;
using Miic.Friends.General.SimpleGroup;
using Miic.Log;
using Miic.Manage.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.AddressBook
{
    public partial class AddressBookInfoDao : IAddressBookInfo
    {
        bool ICommon<AddressBookApplicationInfo>.Insert(AddressBookApplicationInfo application)
        {
            Contract.Requires<ArgumentNullException>(application != null, "参数application:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            try
            {
                result = dbService.Insert(application, out count, out message);
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
                List<AddressBookApplicationInfo> cache = new List<AddressBookApplicationInfo>();
                cache.Add(application);
                InsertCaches(cache);
            }
            return result;
        }

        bool ICommon<AddressBookApplicationInfo>.Update(AddressBookApplicationInfo application)
        {
            Contract.Requires<ArgumentNullException>(application != null, "参数application:不能为空");
            bool result = false;
            int count = 0;
            string message = string.Empty;
            try
            {
                result = dbService.Update(application, out count, out message);
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
                DeleteCache(o => o.ID == application.ID);
            }
            return result;
        }

        bool ICommon<AddressBookApplicationInfo>.Delete(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            bool result = false;
            string message = string.Empty;
            int count = 0;
            try
            {
                result = dbService.Delete(new AddressBookApplicationInfo()
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
                DeleteCache(o => o.ID == id);
            }
            return result;
        }

        AddressBookApplicationInfo ICommon<AddressBookApplicationInfo>.GetInformation(string id)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(id), "参数id:不能为空");
            AddressBookApplicationInfo result = null;
            string message = string.Empty;
            try
            {
                result = subitems.Find(o => o.ID == id);
                if (result == null)
                {
                    result = dbService.GetInformation(new AddressBookApplicationInfo
                    {
                        ID = id
                    }, out message);
                    if (result != null)
                    {
                        List<AddressBookApplicationInfo> cache = new List<AddressBookApplicationInfo>();
                        cache.Add(result);
                        InsertCaches(cache);
                    }
                }
                else
                {
                    string serializer = Config.Serializer.Serialize(result);
                    result = Config.Serializer.Deserialize<AddressBookApplicationInfo>(serializer);
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
        DataTable IAddressBookInfo.GetPersonValidationMessageInfos(string userID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(userID), "参数userID:不能为空");
            string message = string.Empty;
            DataTable result = new DataTable();
            MiicConditionCollections conditions = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition userIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo, string>(o => o.AddresserID),
                userID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, userIDCondition));
            MiicCondition responseStatusCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo, string>(o => o.ResponseStatus),
                ((int)ApplyStatusSetting.Apply).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(responseStatusCondition));
            MiicRelation relation = new MiicRelation(Config.Attribute.GetSqlTableNameByClassName<SimplePersonUserView>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<SimplePersonUserView, string>(o => o.UserID),
                Config.Attribute.GetSqlTableNameByClassName<AddressBookApplicationInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo, string>(o => o.MyUserID),
                MiicDBOperatorSetting.Equal,
                MiicDBRelationSetting.InnerJoin);
            MiicColumnCollections columns = new MiicColumnCollections();
            MiicColumn simpleUserColumns = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<SimplePersonUserView>());
            columns.Add(simpleUserColumns);
            MiicColumn applicationTimeColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AddressBookApplicationInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo, DateTime?>(o => o.ApplicationTime));
            columns.Add(applicationTimeColumn);
            MiicColumn responseTimeColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AddressBookApplicationInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo, DateTime?>(o => o.ResponseTime));
            columns.Add(responseTimeColumn);
            MiicColumn responseStatusColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AddressBookApplicationInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo, string>(o => o.ResponseStatus));
            columns.Add(responseStatusColumn);
            MiicColumn remarkColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AddressBookApplicationInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo, string>(o => o.Remark));
            columns.Add(remarkColumn);
            MiicColumn idColumn = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AddressBookApplicationInfo>(),
               Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo, string>(o => o.ID));
            columns.Add(idColumn);
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


        int IAddressBookInfo.GetPersonValidationMessageCount(string userID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(userID), "参数userID:不能为空");
            string message = string.Empty;
            int result = 0;
            MiicConditionCollections conditions = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition userIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo, string>(o => o.AddresserID),
                userID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, userIDCondition));
            MiicCondition responseStatusCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo, string>(o => o.ResponseStatus),
                ((int)ApplyStatusSetting.Apply).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            conditions.Add(new MiicConditionLeaf(responseStatusCondition));
            MiicColumn column = new MiicColumn(Config.Attribute.GetSqlTableNameByClassName<AddressBookApplicationInfo>(),
                Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookApplicationInfo, string>(o => o.ID));
            try
            {
                result = dbService.GetCount<AddressBookApplicationInfo>(column, conditions, out message);
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
        bool IAddressBookInfo.Agree(ApproveView approveView)
        {
            Contract.Requires<ArgumentNullException>(approveView != null, "参数approveView:不能为空");
            bool result = false;
            MiicConditionCollections condition = approveView.visitor(this);
            string message = string.Empty;
            string message1 = string.Empty;
            string message2 = string.Empty;
            List<string> sqls = new List<string>();
            sqls.Add(DBService.UpdateConditionsSql<AddressBookApplicationInfo>(new AddressBookApplicationInfo()
            {
                ResponseStatus = ((int)ApplyStatusSetting.Agree).ToString(),
                ResponseTime = DateTime.Now
            }, condition, out message1));
            List<AddressBookInfo> addressBookList = new List<AddressBookInfo>();
            //申请者
            addressBookList.Add(new AddressBookInfo()
            {
                ID = Guid.NewGuid().ToString(),
                MyUserID = approveView.MyUserID,
                AddresserID = approveView.ApplicantID,
                AddresserName = approveView.ApplicantName,
                IsBlackList = ((int)MiicYesNoSetting.No).ToString(),
                CanSeeAddresser = ((int)MiicYesNoSetting.Yes).ToString(),
                CanSeeAddresserTime = DateTime.Now,
                CanSeeMe = ((int)MiicYesNoSetting.Yes).ToString(),
                CanSeeMeTime = DateTime.Now,
                ApplicationTime = approveView.ApplicationTime,
                JoinTime = DateTime.Now,
                OftenUsed = ((int)MiicYesNoSetting.No).ToString()
            });
            //接收者
            addressBookList.Add(new AddressBookInfo()
            {
                ID = Guid.NewGuid().ToString(),
                MyUserID = approveView.ApplicantID,
                AddresserID = approveView.MyUserID,
                AddresserName = approveView.MyUserName,
                IsBlackList = ((int)MiicYesNoSetting.No).ToString(),
                CanSeeAddresser = ((int)MiicYesNoSetting.Yes).ToString(),
                CanSeeAddresserTime = DateTime.Now,
                CanSeeMe = ((int)MiicYesNoSetting.Yes).ToString(),
                CanSeeMeTime = DateTime.Now,
                JoinTime = DateTime.Now,
                OftenUsed = ((int)MiicYesNoSetting.No).ToString()
            });
            sqls.Add(DBService.InsertsSql<AddressBookInfo>(addressBookList, out message2));
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
                subitems.RemoveAll(o => o.MyUserID == approveView.ApplicantID && o.AddresserID == approveView.MyUserID);
            }
            return result;
        }

        bool IAddressBookInfo.Refuse(ApproveView approveView)
        {
            Contract.Requires<ArgumentNullException>(approveView != null, "参数approveView:不能为空");
            bool result = false;
            MiicConditionCollections condition = approveView.visitor(this);
            string message = string.Empty;
            int count = 0;
            try
            {
                result = dbService.UpdateConditions<AddressBookApplicationInfo>(new AddressBookApplicationInfo()
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
            if (result == true)
            {
                subitems.RemoveAll(o => o.MyUserID == approveView.ApplicantID && o.AddresserID == approveView.MyUserID);
            }
            return result;
        }
    }
}
