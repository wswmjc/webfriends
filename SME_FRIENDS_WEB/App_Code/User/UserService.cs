using Miic.Base;
using Miic.Base.Setting;
using Miic.BaseStruct;
using Miic.DB.SqlObject;
using Miic.Email;
using Miic.Friends.AddressBook;
using Miic.Friends.Common;
using Miic.Log;
using Miic.Manage.Org;
using Miic.Manage.User;
using Miic.Manage.User.Setting;
using Miic.MiicException;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Transactions;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;

/// <summary>
/// 用户服务 包含登录 查询用户信息 修改用户信息等
/// </summary>
[WebService(Namespace = "http://pyq.mictalk.cn/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[ScriptService]
public class UserService : WebService {

    private static readonly string ClassName = MethodBase.GetCurrentMethod().DeclaringType.Name;
    private static readonly string NamespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
    private  readonly IMiicSocialUser ImiicSocialUser = new MiicSocialUserDao(false);
    private static readonly IUserInfo IuserInfo = new UserInfoDao(false);
    public string UserID { get; private set; }
    public string UserName { get; private set; }
    public UserService()
    {
        string message = string.Empty;
        Cookie cookie = new Cookie();
        this.UserID = cookie.GetCookie("SNS_ID", out message);
        this.UserName = HttpUtility.UrlDecode(cookie.GetCookie("SNS_UserName", out message));
       
    }

    //获取RSA公钥
    [WebMethod(MessageName = "GetRSAPublicKey", EnableSession = true, Description = "获取RSA公钥", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetPublicKey()
    {
        string result = string.Empty;
        RSACryptoServiceProvider rsaKeyGenerator = new RSACryptoServiceProvider(1024);
        string privatekey = privatekey = rsaKeyGenerator.ToXmlString(true);
        //存入私钥
        Context.Session["PrivateKey"] = privatekey;
        RSAParameters publickey = rsaKeyGenerator.ExportParameters(true);
        string publicKeyExponent = Config.Convert.BytesToHexString(publickey.Exponent);
        string publicKeyModulus = Config.Convert.BytesToHexString(publickey.Modulus);
        result = Config.Serializer.Serialize(new { publickeyexponent = publicKeyExponent, publickeymodulus = publicKeyModulus });
        return result;
    }

    [WebMethod(Description = "登录", EnableSession = true, BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(LoginRequestView))]
    [GenerateScriptType(typeof(LoginResponseView))]
    public LoginResponseView Login(LoginRequestView loginView)
    {
        loginView.UserLoginType = UserLoginTypeSetting.Friends;
        LoginResponseView result = ImiicSocialUser.Login(loginView);
        //如果登录成功，清除session
        if (result.IsLogin == true)
        {
            //登录成功，清楚私钥SESSION
            Context.Session.Remove("PrivateKey");
        }
        return result;
    }

    [WebMethod(Description = "通过邮箱找回密码", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool FindPasswordByEmail(string myEmail)
    {
        if (string.IsNullOrEmpty(myEmail) == true)
        {
            throw new ArgumentNullException("myEmail", "参数myEmail：不能为空！");
        }
        bool result = ImiicSocialUser.FindPassword(new FindPasswordView()
        {
            Type = MiicGetBackTypeSetting.Email,
            Value = myEmail,
            LoginType = UserLoginTypeSetting.Friends
        });
      

        return result;
    }

    [WebMethod(BufferResponse = true, Description = "验证密码")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(PasswordView))]
    public bool CheckPassword(PasswordView passwordView)
    {
        bool result = false;
        if (!string.IsNullOrEmpty(UserID))
        {
            MiicSocialUserInfo socialUserInfo = ImiicSocialUser.GetInformation(UserID);
            if (socialUserInfo.Password != passwordView.OldPassword)
            {
                result = false;
            }
            else
            {
                result = true;
            }
        }
        else
        {
            throw new MiicCookieArgumentNullException("UserID失效！");
        }
        return result;
    }
  
   
    [WebMethod(BufferResponse = true, Description = "修改密码")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(PasswordView))]
    public string ModifyPassword(PasswordView passwordView, bool isEmail)
    {
        string result = string.Empty;
        string MailType = "Miic.Config.email.xml";
        string message = string.Empty;
        try
        {
            if (!string.IsNullOrEmpty(passwordView.NewPassword))
            {
                if (!string.IsNullOrEmpty(UserID))
                {
                    MiicSocialUserInfo miicSocialUserInfo = ImiicSocialUser.GetInformation(UserID);
                    miicSocialUserInfo.Password = passwordView.NewPassword;
                    //添加MD5加密密码
                    miicSocialUserInfo.MD5Password = passwordView.Md5;
                    bool temp = ImiicSocialUser.Update(new MiicSocialUserInfo()
                    {
                        ID = miicSocialUserInfo.ID,
                        Password = miicSocialUserInfo.Password,
                        MD5Password = miicSocialUserInfo.MD5Password
                    });
                    if (temp == true)
                    {
                        if (isEmail == true)
                        {
                            bool tempEmail = false;
                            string content = "尊敬的用户" + miicSocialUserInfo.SocialCode + "您好：您的"+Config.AppName+"账户密码已经修改，账号为：" + miicSocialUserInfo.SocialCode + "；新密码为：" + miicSocialUserInfo.Password + "，感谢您的支持，谢谢！";
                            MiicEmail email = new MiicEmail(Config.AppName, "rhadamanthys0407@126.com", miicSocialUserInfo.Email, "找回密码", content);
                            email.Priority = MailPriority.High;
                            Reflection reflection = new Reflection();
                            Assembly a = Assembly.LoadFrom(HttpContext.Current.Server.MapPath("/Bin/MiicLibrary.dll"));
                            Stream stream = a.GetManifestResourceStream(MailType);
                            IEmailService Iemail = reflection.initInterface<IEmailService>(stream, "01");
                            tempEmail = Iemail.SendMail(email, out message);
                            if (tempEmail == true)
                            {
                                result = Config.Serializer.Serialize(new { result = true, message = "您的密码修改成功，请查阅您的Email" });
                            }
                            else
                            {

                                result = Config.Serializer.Serialize(new { result = true, message = "您的密码修改成功，Email发送失败" });
                            }
                        }
                        else
                        {
                            result = Config.Serializer.Serialize(new { result = true, message = "您的密码修改成功" });
                        }
                    }
                    else
                    {
                        result = Config.Serializer.Serialize(new { result = false, message = "您的密码修改失败" });
                    }
                }
                else
                {
                    throw new MiicCookieArgumentNullException("UserID失效！");
                }
            }
            else
            {
                throw new ArgumentNullException("新密码为空！");
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
    /*
    [WebMethod(Description = "用户名唯一性检测", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool UniqueSocialCode(string socialCode)
    {
        return ImiicSocialUser.UniqueSocialCode(socialCode);
    }

    [WebMethod(Description = "获取用户类别绑定", BufferResponse = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MiicKeyValue))]
    public List<MiicKeyValue> GetUserTypeInfos()
    {
        List<MiicKeyValue> result = Config.Attribute.ConvertEnumToList(typeof(UserTypeSetting), e => e.GetDescription());
        result.RemoveAll(o => (o.Name == ((int)UserTypeSetting.AllAdminDeparter).ToString()));
        result.RemoveAll(o => (o.Name == ((int)UserTypeSetting.AllAdminPerson).ToString()));
        result.RemoveAll(o => (o.Name == ((int)UserTypeSetting.AnonymousUser).ToString()));
        result.RemoveAll(o => (o.Name == ((int)UserTypeSetting.All).ToString()));
        return result;
    }

    [WebMethod(BufferResponse = true, Description = "获取用户信息提示")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(UserTypeSetting))]
    public string GetUserInfoTip(string userID, UserTypeSetting userType)
    {
        if (string.IsNullOrEmpty(userID) == true)
        {
            throw new ArgumentNullException("userID", "参数userID：不能为空！");
        }
        string result = "{}";

        if (userType == UserTypeSetting.All
            || userType == UserTypeSetting.AllAdminDeparter
            || userType == UserTypeSetting.AllAdminPerson
            || userType == UserTypeSetting.AnonymousUser)
        {
            throw new ArgumentException("参数userType:传递非法值", "userType");
        }
        DataTable dt = ImiicSocialUser.GetUserMicroblogTip(userID, userType);
        if (dt.Rows.Count == 1)
        {
            dynamic temp;
            if (userType == UserTypeSetting.Administrator || userType == UserTypeSetting.CountryAdmin || userType == UserTypeSetting.LocalAdmin || userType == UserTypeSetting.PersonUser)
            {
                temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<MiicSocialUserInfo, string>(o => o.ID)],
                           ShortRemark = dr[Config.Attribute.GetSqlColumnNameByPropertyName<MiicSocialUserInfo, string>(o => o.Remark)],
                           UserUrl = dr[Config.Attribute.GetSqlColumnNameByPropertyName<MiicSocialUserInfo, string>(o => o.UserUrl)],
                           UserName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<UserInfo, string>(o => o.UserName)],
                           Email = dr[Config.Attribute.GetSqlColumnNameByPropertyName<UserInfo, string>(o => o.Email)],
                           OrgName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<UserInfo, string>(o => o.OrgName)],
                           Tel = dr[Config.Attribute.GetSqlColumnNameByPropertyName<UserInfo, string>(o => o.Tel)],
                           CanSeeTel = dr[Config.Attribute.GetSqlColumnNameByPropertyName<UserInfo, string>(o => o.CanSeeTel)],
                           QQ = dr[Config.Attribute.GetSqlColumnNameByPropertyName<UserInfo, string>(o => o.qq)],
                           CanSeeQQ = dr[Config.Attribute.GetSqlColumnNameByPropertyName<UserInfo, string>(o => o.CanSeeQQ)],
                           Mobile = dr[Config.Attribute.GetSqlColumnNameByPropertyName<UserInfo, string>(o => o.Mobile)],
                           CanSeeMobile = dr[Config.Attribute.GetSqlColumnNameByPropertyName<UserInfo, string>(o => o.CanSeeMobile)],
                           FansCount = dr["FANS_COUNT"],
                           AttentionersCount = dr["ATTENTIONERS_COUNT"],
                           MicroblogCount = dr["MICROBLOG_COUNT"]
                       };
            }
            else if (userType == UserTypeSetting.OrgUser || userType == UserTypeSetting.LocalDepartUser || userType == UserTypeSetting.CountryDepartUser)
            {
                temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID = dr[Config.Attribute.GetSqlColumnNameByPropertyName<MiicSocialUserInfo, string>(o => o.ID)],
                           ShortRemark = dr[Config.Attribute.GetSqlColumnNameByPropertyName<MiicSocialUserInfo, string>(o => o.Remark)],
                           OrgName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<OrganizationInfo, string>(o => o.OrgName)],
                           Email = dr[Config.Attribute.GetSqlColumnNameByPropertyName<OrganizationInfo, string>(o => o.Email)],
                           LinkMan = dr[Config.Attribute.GetSqlColumnNameByPropertyName<OrganizationInfo, string>(o => o.LinkMan)],
                           Tel = dr[Config.Attribute.GetSqlColumnNameByPropertyName<OrganizationInfo, string>(o => o.Tel)],
                           Mobile = dr[Config.Attribute.GetSqlColumnNameByPropertyName<OrganizationInfo, string>(o => o.Mobile)],
                           WebSite = dr[Config.Attribute.GetSqlColumnNameByPropertyName<OrganizationInfo, string>(o => o.WebSite)],
                           FansCount = dr["FANS_COUNT"],
                           AttentionersCount = dr["ATTENTIONERS_COUNT"],
                           MicroblogCount = dr["MICROBLOG_COUNT"]
                       };

            }
            else
            {
                temp = result;
            }
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
      */
    [WebMethod(BufferResponse = true, Description = "获取某人的统计信息")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public List<MiicKeyValue> GetPersonStatisticsCount(string userID) 
    {
        Miic.Friends.User.IUserInfo IuserInfo = new Miic.Friends.User.UserInfoDao();
        return IuserInfo.GetPersonStatisticsCount(userID);
    }
    [WebMethod(BufferResponse = true, Description = "搜索用户信息(添加好友使用)")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MyKeywordView))]
    [GenerateScriptType(typeof(MiicPage))]
    public string SearchFriends(MyKeywordView keywordView, MiicPage page) 
    {
        string result = CommonService.InitialJsonList;
        Miic.Friends.User.IUserInfo IuserInfo = new Miic.Friends.User.UserInfoDao();
        DataTable dt = IuserInfo.Search(keywordView, page);
        if (dt.Rows.Count > 0) 
        {
            var temp = from dr in dt.AsEnumerable()
                       select new
                       {
                           ID=dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.User.SimpleUserView,string>(o=>o.UserID)],
                           UserType = dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.User.SimpleUserView, string>(o => o.UserType)],
                           UserName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.User.SimpleUserView, string>(o => o.UserName)],
                           Sex = dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.User.SimpleUserView, string>(o => o.Sex)],
                           OrgName = dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.User.SimpleUserView, string>(o => o.OrgName)],
                           UserUrl = CommonService.GetManageFullUrl(dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.User.SimpleUserView, string>(o => o.UserUrl)].ToString()),
                           IsMyFriend = Convert.IsDBNull(dr["ADDRESS_BOOK_" + Config.Attribute.GetSqlColumnNameByPropertyName<AddressBookInfo, string>(o => o.ID)]) == false ||(!string.IsNullOrEmpty(this.UserID)&& dr[Config.Attribute.GetSqlColumnNameByPropertyName<Miic.Friends.User.SimpleUserView, string>(o => o.UserID)].ToString()==this.UserID)?  true:false 
                       };
            result = Config.Serializer.Serialize(temp);
        }
        return result;
    }
    [WebMethod(BufferResponse = true, Description = "搜索用户数(添加好友使用)")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    [GenerateScriptType(typeof(MyKeywordView))]
    public int GetSearchFriendsCount(MyKeywordView keywordView) 
    {
        Miic.Friends.User.IUserInfo IuserInfo = new Miic.Friends.User.UserInfoDao();
        return IuserInfo.GetSearchCount(keywordView);
    }

}
