using Miic.Base;
using Miic.Common;
using Miic.DB.SqlObject;
using Miic.Friends.Common;
using Miic.Friends.Notice;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Community
{
    public interface IPublishInfo:ICommon<PublishInfo>,ICommon<AccessoryInfo>
    {
        /// <summary>
        /// 根据行业圈子获取所有发布博文附件列表
        /// </summary>
        /// <param name="communityID">行业圈子ID</param>
        /// <returns>附件列表</returns>
        List<AccessoryInfo> GetAccessoryListByCommunityID(string communityID);
        /// <summary>
        /// 根据标签主题获取所有发布博文附件列表
        /// </summary>
        /// <param name="labelID">标签主题ID</param>
        /// <returns>附件列表</returns>
        List<AccessoryInfo> GetAccessoryListByLabelID(string labelID);
        /// <summary>
        /// 获取某人的某圈子发布的信息
        /// </summary>
        /// <param name="dateView">日期视图</param>
        /// <param name="page">分页，默认不分页</param>
        /// <returns>发布信息列表</returns>
        DataTable GetCommunityPublishInfos(CommunityDateView dateView, MiicPage page);
        /// <summary>
        /// 获取自己所在的所有圈子发布信息数
        /// </summary>
        /// <param name="dateView">日期视图</param>
        /// <returns>发布信息列表数</returns>
        int GetCommunityPublishCount(CommunityDateView dateView);
        /// <summary>
        /// 获取自己最早发布的行业圈子内的信息
        /// </summary>
        /// <param name="top">条目数，默认为1</param>
        /// <param name="isSelf">是否是自己发布的，默认为true</param>
        /// <returns>发布信息列表</returns>
        DataTable GetOldestCommunityPubilishInfos(CommunityTopView topView);
         /// <summary>
        /// 获取年份列表
        /// </summary>
        /// <param name="dateView"></param>
        /// <returns></returns>
        List<string> GetCommunityPublishInfosYearList(CommunityDateView dateView);
        /// <summary>
        /// 获取月份列表
        /// </summary>
        /// <param name="dateView"></param>
        /// <returns></returns>
        List<string> GetCommunityPublishInfosMonthList(CommunityDateView dateView);
        /// <summary>
        /// 根据ID获取详细信息内容
        /// </summary>
        /// <param name="ID">信息ID</param>
        /// <returns>详细信息</returns>
        DataTable GetDetailPublishInfo(string id);
        /// <summary>
        /// 设置已经发布的行业圈子信息上下线
        /// </summary>
        /// <param name="ID">上下线状态视图</param>
        /// <returns>Yes/No</returns>
        bool SetEditStatus(EditStatusView editStatusView);
        /// <summary>
        /// 新增行业圈子信息
        /// </summary>
        /// <param name="publishInfo">信息</param>
        /// <param name="accessoryInfos">信息附件，可为空</param>
        /// <param name="simpleLabelViews">信息标签，可为空</param>
        /// <param name="noticeUserView">提醒人，可为空</param>
        /// <returns></returns>
        bool Insert(PublishInfo publishInfo, List<AccessoryInfo> accessoryInfos = null, List<SimpleLabelView> simpleLabelViews = null, NoticeUserView noticeUserView = null);
        /// <summary>
        /// 修改行业圈子信息
        /// </summary>
        /// <param name="publishInfo">信息</param>
        /// <param name="accessoryInfos">信息附件，可为空</param>
        /// <param name="simpleLabelViews">信息标签，可为空</param>
        /// <param name="removeSimpleAccessoryViews">删除附件，可为空</param>
        /// <param name="removeSimpleLabelViewIDs">删除标签，可为空</param>
        /// <param name="noticeUserView">提醒人，可为空</param>
        /// <returns></returns>
        bool Update(PublishInfo publishInfo, List<AccessoryInfo> accessoryInfos = null, List<SimpleLabelView> simpleLabelViews = null, List<SimpleAccessoryView> removeSimpleAccessoryViews = null, List<string> removeSimpleLabelViewIDs = null, NoticeUserView noticeUserView = null);     
        /// <summary>
        /// 搜索某人草稿列表
        /// </summary>
        /// <param name="keywordView">关键字视图</param>
        /// <param name="page">分页项</param>
        /// <returns>某人草稿列表</returns>
        DataTable GetDraftInfos(DraftSearchView keywordView, MiicPage page = null);  
        /// <summary>
        /// 获取某人草稿总数
        /// </summary>
        /// <param name="keywordView">关键字视图</param>
        /// <returns>个人草稿总数</returns>
        int GetDraftInfoCount(DraftSearchView keywordView);
        /// <summary>
        /// 是否有行业圈子发布的内容
        /// </summary>
        /// <param name="communityID">行业圈子ID</param>
        /// <returns>Yes/No</returns>
        bool HasCommunityPublish(string communityID);
        /// <summary>
        /// 获取用户对于某行业圈文章的行为状态
        /// </summary>
        /// <param name="behaviorView">用户行为视图</param>
        /// <returns>用户对于某行业圈文章的行为状态（是否点赞、是否点踩、是否举报、是否收藏）</returns>
        DataTable GetMyCommunityBehaviorFlags(MyCommunityBehaviorView behaviorView);
    }
}
