using Miic.Base;
using Miic.DB.SqlObject;
using Miic.Friends.Common;
using Miic.Friends.Community.Setting;
using Miic.Friends.General.SimpleGroup;
using System.Collections.Generic;
using System.Data;

namespace Miic.Friends.Community
{
    public interface ICommunityInfo : ICommon<CommunityInfo>, ICommon<CommunityMember>
    {
        /// <summary>
        /// 搜索我的/某人的圈子信息列表
        /// </summary>
        /// <param name="searchView">搜索视图</param>
        /// <param name="page">分页，默认不分页</param>
        /// <returns>圈子信息列表</returns>
        DataTable Search(GeneralSimpleGroupSearchView searchView, MiicPage page = null);
        /// <summary>
        /// 搜索我的/某人的圈子数
        /// </summary>
        /// <param name="searchView">搜索视图</param>
        /// <returns>圈子数</returns>
        int GetSearchCount(GeneralSimpleGroupSearchView searchView);
        /// <summary>
        /// 搜索我的/某人行业圈子动态信息查询
        /// </summary>
        /// <param name="searchView">搜索视图</param>
        /// <param name="page">分页，默认不分页</param>
        /// <returns>行业圈子以及信息列表</returns>
        DataTable PersonTrendsSearch(GeneralSimpleGroupSearchView searchView, MiicPage page = null);
        /// <summary>
        /// 获取我的/某人含有行业圈子动态信息的讨论组数
        /// </summary>
        /// <param name="searchView">搜索视图</param>
        /// <returns>我的/某人行业圈子动态信息的讨论组数</returns>
        int GetPersonTrendsSearchCount(GeneralSimpleGroupSearchView searchView);
        /// <summary>
        /// 获取某人的行业圈子列表(主页右侧显示)
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="top">top 默认8</param>
        /// <returns>行业圈子列表</returns>
        DataTable GetCommunityInfoList(string userID, int top = 8);
        /// <summary>
        /// 判断是否有行业圈子
        /// </summary>
        /// <param name="userID">用户ID</param> 
        /// <returns>Yes/No</returns>
        bool HaveCommunity(string userID);
        /// <summary>
        /// 搜索热门圈子信息列表
        /// </summary>
        /// <param name="keywordView">关键字视图</param>
        /// <param name="page">分页</param>
        /// <returns>热门圈子信息列表</returns>
        DataTable SearchHotCommunity(MyKeywordView keywordView, MiicPage page = null);
        /// <summary>
        /// 获取搜索热门圈子信息列表数
        /// </summary>
        /// <param name="keywordView">关键字视图</param>
        /// <returns>热门圈子信息列表数</returns>
        int GetSearchHotCommunityCount(Miic.Friends.Common.NoPersonKeywordView keywordView);
        /// <summary>
        /// 获取热门圈子列表(主页右侧显示)
        /// </summary>
        /// <param name="top">top 默认8</param>
        /// <returns>热门圈子列表</returns>
        DataTable GetHotCommunityRecommendInfoList(int top = 8);
        /// <summary>
        /// 新增行业圈子
        /// </summary>
        /// <param name="communityInfo">行业圈子信息</param>
        /// <param name="memberApplications">成员邀请列表</param>
        /// <returns>Yes/No</returns>
        bool Insert(CommunityInfo communityInfo, List<CommunityApplicationInfo> memberApplications);
        /// <summary>
        /// 更新行业圈子信息
        /// </summary>
        /// <param name="communityInfo">行业圈子信息</param>
        /// <param name="members">成员信息</param>
        /// <returns>Yes/No</returns>
        bool Update(CommunityInfo communityInfo, List<CommunityMember> members);
        /// <summary>
        /// 成员申请
        /// </summary>
        /// <param name="memberApplications">申请人信息集合</param>
        /// <returns>Yes/No</returns>
        bool MemberApply(List<CommunityApplicationInfo> memberApplications);
        /// <summary>
        /// 邀请成员
        /// </summary>
        /// <param name="memberApplications">申请人信息集合</param>
        /// <returns>Yes/No</returns>
        bool MemberInvite(List<CommunityApplicationInfo> memberApplications);
        /// <summary>
        /// 拒绝成员加入/邀请
        /// </summary>
        ///<param name="approveView">审批视图</param>
        /// <returns>Yes/No</returns>
        bool MemberRefuse(ApproveView approveView);
        /// <summary>
        /// 同意加入/邀请
        /// </summary>
        /// <param name="approveView">审批视图</param>
        /// <returns>Yes/No</returns>
        bool MemberAgree(ApproveView approveView);
        /// <summary>
        /// 成员信息校验忽略
        /// </summary>
        /// <param name="applyID">申请ID</param>
        /// <returns>Yes/No</returns>
        bool MemberIgnore(string applyID);
        /// <summary>
        /// 批量删除成员
        /// </summary>
        /// <param name="communityID">圈子ID</param>
        /// <param name="memberIDs">成员ID</param>
        /// <returns></returns>
        bool RemoveMembers(string communityID,List<string> memberIDs);
        /// <summary>
        /// 删除行业圈子
        /// </summary>
        /// <param name="id">行业圈子id</param>
        /// <returns>Yes/No</returns>
        /// <remarks>级联成员、发布内容、标签主题等</remarks>
        bool DeepDelete(string id);
        /// <summary>
        /// 是否为圈子的管理员
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="communityID">圈子ID，默认为空</param>
        /// <returns>Yes/No</returns>
        /// <remarks>如果圈子ID为空，则表示查所有圈子中是否为管理员</remarks>
        bool IsAdmin(string userID,string communityID="");
        /// <summary>
        /// 是否为圈子的创建者
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="communityID">圈子ID</param>
        /// <returns>Yes/No</returns>
        bool IsCreater(string userID, string communityID);
        /// <summary>
        /// 是否为圈子的成员
        /// </summary>
        /// <param name="memberID">用户ID</param>
        /// <param name="communityID">圈子ID</param>
        /// <returns>Yes/No</returns>
        bool IsMember(string memberID, string communityID);
        /// <summary>
        /// 根据行业圈子ID获取圈子信息列表
        /// </summary>
        /// <param name="communityID">行业圈子ID</param>
        /// <returns>行业圈子成员列表</returns>
        List<CommunityMember> GetMemberInfoListByCommunityID(string communityID);
        /// <summary>
        /// 根据行业圈子ID获取圈子信息列表(含成员头像)
        /// </summary>
        /// <param name="communityID"></param>
        /// <returns>行业圈子成员列表</returns>
        DataTable GetDetailMemberInfoListByCommunityID(string communityID);
        /// <summary>
        /// 获取某人的通知信息数
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns>通知消息数</returns>
        int GetPersonValidationMessageCount(string userID);
        /// <summary>
        /// 获取某人的通知信息
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns>通知消息</returns>
        DataTable GetPersonValidationMessageInfos(string userID);
        /// <summary>
        /// 获取某人的通讯录信息（不包括该圈子的成员）
        /// </summary>
        /// <param name="communityID">行业圈子ID</param>
        /// <param name="userID">用户ID</param>
        /// <returns>通讯录列表</returns>
        DataTable GetPersonAddressBookList(string communityID,string userID);
        /// <summary>
        /// 某人退出行业圈子
        /// </summary>
        /// <param name="communityID">行业圈子ID</param>
        /// <param name="userID">用户ID</param>
        /// <returns>Yes/No</returns>
        bool PersonQuit(string communityID, string userID);
        /// <summary>
        /// 根据标签获取发布信息列表
        /// </summary>
        /// <param name="labelID">标签ID</param>
        /// <param name="page">分页，默认不分页</param>
        /// <returns>发布信息列表</returns>
        DataTable GetPublishInfosByLabelID(string labelID,MiicPage page);
        /// <summary>
        /// 根据标签获取发布信息列表数
        /// </summary>
        /// <param name="labelID">标签ID</param>
        /// <returns>发布信息列表数</returns>
        int GetPublishInfosCountByLabelID(string labelID);
        /// <summary>
        /// 是否能够建立圈子
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns>Yes/No</returns>
        bool CanCreateCommunity(string userID);
        /// <summary>
        /// 获取行业圈子统计信息
        /// </summary>
        /// <returns></returns>
        DataTable GetCommunityToPersonStatistics(StatisticsSearchView searchView);
        /// <summary>
        /// 获取行业圈子-标签统计信息
        /// </summary>
        /// <param name="searchView"></param>
        /// <returns></returns>
        DataTable GetCommunityToLabelStatistics(StatisticsSearchView searchView);
        /// <summary>
        /// 获取所有行业圈子
        /// </summary>
        /// <returns></returns>
        DataTable GetAllCommunityWithLabelList();
        /// <summary>
        /// 申请是否被处理
        /// </summary>
        /// <param name="approveView">审批视图</param>
        /// <returns></returns>
        bool IsApplicationHandled(ApproveView approveView);
        /// <summary>
        /// 申请是否被处理
        /// </summary>
        /// <param name="applyID">申请ID</param>
        /// <returns></returns>
        bool IsApplicationHandledByApplyID(string applyID);
    }
}
