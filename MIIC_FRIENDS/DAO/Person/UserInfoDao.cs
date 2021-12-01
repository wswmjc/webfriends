using Miic.Base;
using Miic.Base.Setting;
using Miic.BaseStruct;
using Miic.DB;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using Miic.Friends.AddressBook;
using Miic.Friends.Common;
using Miic.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.User
{
    public partial class UserInfoDao : IUserInfo
    {
        private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
        private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
        private static DBService dbService = new DBService();
        List<MiicKeyValue> IUserInfo.GetPersonStatisticsCount(string userID)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(userID), "参数userID:不能为空");
            List<MiicKeyValue> result = new List<MiicKeyValue>();
            DataTable dt = new DataTable();
            string message = string.Empty;
            try
            {
                dt = dbService.querySql("select * from GetPersonStatisticsCount('" + userID + "')", out message);
                foreach (var dr in dt.AsEnumerable())
                {
                    result.Add(new MiicKeyValue()
                    {
                        Name = dr["ID"].ToString(),
                        Value = dr["STATISTICS_COUNT"]
                    });
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



        DataTable IUserInfo.Search(KeywordView keywordView, MiicPage page)
        {
            Contract.Requires<ArgumentNullException>(keywordView != null, "参数keywordView:不能为空");
            DataTable result = new DataTable();
            string message = string.Empty;
            string sql = string.Empty;
            string contentSql = "select "+Config.Attribute.GetSqlTableNameByClassName<SimpleUserView>()+".* ,TEMP_ADDRESS_BOOK_INFO.ID as ADDRESS_BOOK_ID";
            contentSql += " from " + Config.Attribute.GetSqlTableNameByClassName<SimpleUserView>() + " left join (select ID,ADDRESSER_ID from " + Config.Attribute.GetSqlTableNameByClassName<AddressBookInfo>();
            contentSql+=" where ("+Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo,string>(o=>o.MyUserID)+" = '"+keywordView.userID+"')) as TEMP_ADDRESS_BOOK_INFO  ";
            contentSql+=" on "+Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<SimpleUserView,string>(o=>o.UserID)+"=TEMP_ADDRESS_BOOK_INFO."+Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo,string>(o=>o.AddresserID);
            contentSql += " where ("+Config.Attribute.GetSqlColumnNameByPropertyName<SimpleUserView,string>(o=>o.UserName)+"  like  '%"+keywordView.Keyword+"%') ";
            contentSql += " and SIMPLE_USER_VIEW.USER_ID in (select ID from MIIC_SOCIAL_COMMON.dbo.MIIC_SOCIAL_USER where CAN_SEARCH = '"+((int)MiicYesNoSetting.Yes).ToString()+"') ";
            try
            {
                if (page != null)
                {
                    sql = "with INFO_PAGE as ( select row_number()  over ( ORDER BY Temp.USER_NAME ASC) as row,Temp.* from ( ";
                    sql += contentSql;
                    sql += ") as Temp)";
                    sql += "select * from INFO_PAGE where row between " + page.pageStart + " and " + page.pageEnd;
                }
                else
                {
                    sql = contentSql;
                   
                }
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

        int IUserInfo.GetSearchCount(KeywordView keywordView)
        {
            int result = 0;
            string message = string.Empty;

            string sql = string.Empty;
            sql += "select count(" + Config.Attribute.GetSqlTableNameByClassName<SimpleUserView>() + ".USER_ID) ";
            sql += " from " + Config.Attribute.GetSqlTableNameByClassName<SimpleUserView>() + " left join (select ID,ADDRESSER_ID from " + Config.Attribute.GetSqlTableNameByClassName<AddressBookInfo>();
            sql += " where (" + Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.MyUserID) + " = '" + keywordView.userID + "')) as TEMP_ADDRESS_BOOK_INFO  ";
            sql += " on " + Config.Attribute.GetSqlColumnNameByPropertyNameWithTable<SimpleUserView, string>(o => o.UserID) + "=TEMP_ADDRESS_BOOK_INFO." + Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.AddresserID);
            sql += " where  (" + Config.Attribute.GetSqlColumnNameByPropertyName<SimpleUserView, string>(o => o.UserName) + "  like  '%" + keywordView.Keyword + "%') ";
            sql += " and SIMPLE_USER_VIEW.USER_ID in (select ID from MIIC_SOCIAL_COMMON.dbo.MIIC_SOCIAL_USER where CAN_SEARCH = '"+((int)MiicYesNoSetting.Yes).ToString()+"') ";
            
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
