using Miic.Base;
using Miic.Common;
using Miic.DB.SqlObject;
using Miic.Friends.Common;
using Miic.Friends.Community;
using Miic.Friends.Notice;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Moments
{
    public interface IPublishInfo : ICommon<PublishInfo>, ICommon<AccessoryInfo>
    {
        /// <summary>
        /// 获取某人的朋友圈发布信息
        /// </summary>
        /// <param name="dateView">日期视图</param>
        /// <param name="page">分页，默认不分页</param>
        /// <returns>发布信息列表</returns>
        DataTable GetPersonMomentsPublishInfos(GeneralDateView dateView, MiicPage page = null);
        /// <summary>
        /// 获取某人的朋友圈发布信息数
        /// </summary>
        /// <param name="dateView">日期视图</param>
        /// <returns>发布信息列表数</returns>
        int GetPersonMomentsPublishCount(GeneralDateView dateView);
        /// <summary>
        /// 获取年份列表
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        List<string> GetPersonMomentsPublishInfosYearList(GeneralDateView dateView);
        /// <summary>
        /// 获取月份列表
        /// </summary>
        /// <param name="year"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        List<string> GetPersonMomentsPublishInfosMonthList(GeneralDateView dateView);
        /// <summary>
        /// 获取最早发布的朋友圈信息
        /// </summary>
        /// <param name="topView">条件视图</param>
        /// <returns>发布信息列表</returns>
        DataTable GetOldestMomentsPubilishInfos(GeneralTopView topView);
        /// <summary>
        /// 获取最新发布的朋友圈信息
        /// </summary>
        /// <param name="topView">条件视图</param>
        /// <returns>发布信息列表</returns>
        DataTable GetNewestMomentsPublishInfos(GeneralTopView topView);
        /// <summary>
        /// 根据ID获取详细信息内容
        /// </summary>
        /// <param name="ID">信息ID</param>
        /// <returns>详细信息</returns>
        DataTable GetDetailPublishInfo(string id);
        /// <summary>
        /// 设置已经发布的微博上下线
        /// </summary>
        /// <param name="ID">上下线状态视图</param>
        /// <returns>Yes/No</returns>
        bool SetEditStatus(EditStatusView editStatusView);
       
        /// <summary>
        /// 新增信息
        /// </summary>
        /// <param name="publishInfo">信息</param>
        /// <param name="noticeUserView">提醒人视图</param>
        /// <returns>Yes/No</returns>
        bool Insert(PublishInfo publishInfo, NoticeUserView noticeUserView);
        /// <summary>
        /// 新增信息
        /// </summary>
        /// <param name="publishInfo">信息</param>
        /// <param name="noticeUserView">提醒人视图</param>
        /// <param name="publishAccessoryInfo">信息附件</param>
        /// <returns>Yes/No</returns>
        bool Insert(PublishInfo publishInfo, NoticeUserView noticeUserView, List<AccessoryInfo> publishAccessoryInfos);
        /// <summary>
        /// 更新信息
        /// </summary>
        /// <param name="publishInfo">信息</param>
        /// <param name="removeSimpleAccessoryViews">删除附件队列</param>
        /// <param name="noticeUserView">提醒人视图,可为空</param>
        /// <returns>Yes/No</returns>
        bool Update(PublishInfo publishInfo, List<SimpleAccessoryView> removeSimpleAccessoryViews, NoticeUserView noticeUserView = null);
        /// <summary>
        /// 更新信息
        /// </summary>
        /// <param name="publishInfo">信息</param>
        /// <param name="publishAccessoryInfos">信息附件</param>
        /// <param name="removeSimpleAccessoryViews">删除附件队列</param>
        /// <param name="noticeUserView">提醒人视图，可为空</param>
        /// <returns></returns>
        bool Update(PublishInfo publishInfo, List<AccessoryInfo> publishAccessoryInfos, List<SimpleAccessoryView> removeSimpleAccessoryViews = null, NoticeUserView noticeUserView = null);
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
        /// 获取某人最新朋友圈信息（条件：长篇、已发布、有效的、上线的）
        /// </summary>
        /// <param name="top">默认：15</param>
        /// <returns>热门博文列表</returns>
        DataTable GetTopSimpleMomentsInfos(string userID, int top = 15);
        /// <summary>
        /// 获取用户对于某朋友圈文章的行为状态
        /// </summary>
        /// <param name="behaviorView">用户行为视图</param>
        /// <returns>用户对于某朋友圈文章的行为状态（是否点赞、是否点踩、是否举报、是否收藏）</returns>
        DataTable GetMyMomentsBehaviorFlags(MyBehaviorView behaviorView);
    }
}
