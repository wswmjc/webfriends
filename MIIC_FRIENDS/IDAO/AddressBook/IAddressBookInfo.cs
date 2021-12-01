using Miic.Base;
using Miic.DB.SqlObject;
using Miic.Friends.General.SimpleGroup;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.AddressBook
{
   public   interface IAddressBookInfo:ICommon<AddressBookInfo>,ICommon<AddressBookApplicationInfo>
    {
       /// <summary>
       /// 获取某人的经常使用的通讯录
       /// </summary>
       /// <param name="userID">用户ID</param>
       /// <param name="top">top（默认：15）</param>
       /// <returns>经常使用的通讯录列表</returns>
       DataTable GetOffenUsedAddressBookList(string userID,int top=15);
       /// <summary>
       /// 搜索我的/某人的通讯录组信息列表
       /// </summary>
       /// <param name="searchView">搜索视图</param>
       /// <param name="page">分页，默认不分页</param>
       /// <returns>通讯录信息列表</returns>
       DataTable Search(GeneralSimpleGroupSearchView searchView,MiicPage page=null);
       /// <summary>
       /// 搜索我的/某人的通讯录数
       /// </summary>
       /// <param name="searchView">搜索视图</param>
       /// <returns>通讯录数</returns>
       int GetSearchCount(GeneralSimpleGroupSearchView searchView);
       /// <summary>
       /// 获取我的/某人的通讯录组的黑名单列表
       /// </summary>
       /// <param name="searchView">搜索视图</param>
       /// <param name="page">分页，默认不分页</param>
       /// <returns>通讯录黑名单列表</returns>
       DataTable GetPersonBlackListInfos(GeneralSimpleGroupSearchView searchView, MiicPage page = null);
       /// <summary>
       /// 获取我的/某人的通讯录组的黑名单列表数
       /// </summary>
       /// <param name="searchView">搜索视图</param>
       /// <returns>通讯录黑名单列表数</returns>
       int GetPersonBlackListCount(GeneralSimpleGroupSearchView searchView);
       /// <summary>
       /// 设置备注
       /// </summary>
       /// <param name="remarkView">备注视图</param>
       /// <returns>Yes/No</returns>
       bool SetRemark(SetRemarkView remarkView);
       /// <summary>
       /// 设置是否可以看我的朋友圈
       /// </summary>
       /// <param name="canSeeMeView">是否可以看我朋友圈设置视图</param>
       /// <returns>Yes/No</returns>
       bool SetCanSeeMe(SetCanSeeMeView canSeeMeView);
       /// <summary>
       /// 设置是否看他人朋友圈
       /// </summary>
       /// <param name="canSeeAddresserView">是否看他人朋友圈设置视图</param>
       /// <returns>Yes/No</returns>
       bool SetCanSeeAddresser(SetCanSeeAddresserView canSeeAddresserView);
       /// <summary>
       /// 设置是否经常使用
       /// </summary>
       /// <param name="oftenUsedView">是否经常使用视图</param>
       /// <returns>Yes/No</returns>
       bool SetOftenUsed(SetOftenUsedView oftenUsedView);
      
       /// <summary>
       /// 插入通讯录
       /// </summary>
       /// <param name="ID">ID</param>
       /// <returns>Yes/No</returns>
       bool Insert(string ID);
       /// <summary>
       /// 获取某人的通知信息
       /// </summary>
       /// <param name="userID">用户ID</param>
       /// <returns>通知消息</returns>
       DataTable GetPersonValidationMessageInfos(string userID);
       /// <summary>
       /// 获取某人的通知信息数
       /// </summary>
       /// <param name="userID">用户ID</param>
       /// <returns>通知消息数</returns>
       int GetPersonValidationMessageCount(string userID);
       /// <summary>
       /// 同意添加通讯录
       /// </summary>
       /// <param name="approveView">审批视图</param>
       /// <returns>Yes/No</returns>
       bool Agree(ApproveView approveView);
       /// <summary>
       /// 拒绝添加通讯录
       /// </summary>
       /// <param name="approveView">审批视图</param>
       /// <returns>Yes/No</returns>
       bool Refuse(ApproveView approveView);
       /// <summary>
       /// 解除通讯录关系
       /// </summary>
       /// <param name="firestUserID">用户1ID</param>
       /// <param name="secondUserID">用户2ID</param>
       /// <returns></returns>
       bool Remove(string firestUserID, string secondUserID);
       /// <summary>
       /// 是否联系人对自己可见
       /// </summary>
       /// <param name="myID"></param>
       /// <param name="addresserID"></param>
       /// <returns></returns>
       bool CanSeeAddresser(string myID, string addresserID);
       /// <summary>
       /// 获取某人的所有经常联系通讯录列表
       /// </summary>
       /// <param name="userID">用户ID</param>
       /// <returns>经常使用的通讯录列表</returns>
       DataTable GetAllOftenUsedAddressBookList(string userID);
    }
}
