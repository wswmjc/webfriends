using Miic.Base;
using Miic.DB.SqlObject;
using Miic.Friends.General.SimpleGroup;
using Miic.Friends.SimpleGroup;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Group
{
    public  interface IGroupInfo:ICommon<GroupInfo>,ICommon<GroupMember>
    {
        /// <summary>
        /// 搜索我的/某人的讨论组信息列表
        /// </summary>
        /// <param name="searchView">搜索视图</param>
        /// <param name="page">分页，默认不分页</param>
        /// <returns>讨论组组信息列表</returns>
        DataTable Search(GeneralSimpleGroupSearchView searchView,MiicPage page=null);
        /// <summary>
        /// 搜索我的/某人的讨论组数
        /// </summary>
        /// <param name="searchView">搜索视图</param>
        /// <returns>讨论组数</returns>
        int GetSearchCount(GeneralSimpleGroupSearchView searchView);
        /// <summary>
        /// 搜索我的/某人讨论组动态信息查询
        /// </summary>
        /// <param name="searchView">搜索视图</param>
        /// <param name="page">分页，默认不分页</param>
        /// <returns>讨论组以及信息列表</returns>
        DataTable TrendsSearch(GeneralSimpleGroupSearchView searchView, MiicPage page = null);
        /// <summary>
        /// 获取我的/某人含有讨论组动态信息的讨论组数
        /// </summary>
        /// <param name="searchView">搜索视图</param>
        /// <returns>我的/某人讨论组动态信息的讨论组数</returns>
        int GetTrendsSearchCount(GeneralSimpleGroupSearchView searchView);
        /// <summary>
        /// 获取某人的讨论组列表(主页右侧显示)
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="top">top 默认8</param>
        /// <returns>讨论组列表</returns>
        DataTable GetGroupInfoList(string userID, int top = 8);
        /// <summary>
        /// 判断是否有讨论组
        /// </summary>
        /// <param name="userID">用户ID</param> 
        /// <returns>Yes/No</returns>
        bool HaveGroup(string userID);
        /// <summary>
        /// 新增讨论组
        /// </summary>
        /// <param name="groupInfo">讨论组信息</param>
        /// <param name="members">成员列表</param>
        /// <returns>Yes/No</returns>
        bool Insert(GroupInfo groupInfo, List<GroupMember> members);
        /// <summary>
        /// 新增讨论组成员
        /// </summary>
        /// <param name="groupID">讨论组ID</param>
        /// <param name="members">成员列表</param>
        /// <returns>Yes/No</returns>
        bool Insert(string groupID, List<GroupMember> members);
        /// <summary>
        /// 删除讨论组成员
        /// </summary>
        /// <param name="groupID">讨论组ID</param>
        /// <param name="memberIDs">成员ID集合</param>
        /// <returns>Yes/No</returns>
        bool Delete(string groupID, List<string> memberIDs);
        /// <summary>
        /// 设置备注
        /// </summary>
        /// <param name="remarkView">备注视图</param>
        /// <returns>Yes/No</returns>
        bool SetRemark(SetRemarkView remarkView);
        /// <summary>
        /// 根据讨论组ID获取所有讨论组成员
        /// </summary>
        /// <param name="groupID">讨论组ID</param>
        /// <returns>讨论组成员列表</returns>
        List<GroupMember> GetGroupMemberList(string groupID);
        /// <summary>
        /// 根据讨论组ID获取所有讨论组成员(含成员头像)
        /// </summary>
        /// <param name="groupID"></param>
        /// <returns>讨论组成员列表</returns>
        DataTable GetDetailMemberInfoListByGroupID(string groupID);
        /// <summary>
        /// 是否为讨论组的创建者
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="groupID">讨论组ID</param>
        /// <returns>Yes/No</returns>
        bool IsCreater(string userID, string groupID);
        /// <summary>
        /// 根据讨论ID查询组信息
        /// </summary>
        /// <param name="topicID"></param>
        /// <returns></returns>
        DataTable GetDetailGroupByTopicID(string topicID);
        /// <summary>
        /// 查找不在小组内的查询者联系人列表
        /// </summary>
        /// <param name="groupSearchView">查询视图</param>
        /// <returns>联系人列表</returns>
        DataTable GetInvitingAddressList(MySimpleGroupSearchView groupSearchView,MiicPage page = null);
        /// <summary>
        /// 查询不在小组内的查询者联系人数量
        /// </summary>
        /// <param name="groupSearchView"></param>
        /// <returns></returns>
        int GetInvitingAddresserCount(MySimpleGroupSearchView groupSearchView);
    }
}
