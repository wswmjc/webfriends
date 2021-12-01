using Miic.Base;
using Miic.DB.SqlObject;
using Miic.Friends.Notice;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Miic.Friends.Community
{
    public interface IMessageInfo:ICommon<MessageInfo>,ICommon<TopicInfo>
    {
        /// <summary>
        /// 删除Topic信息(管理员使用)
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>Yes/No</returns>
        /// <remarks>包括Message的内容，级联</remarks>
        bool DeepDelete(string id);
        /// <summary>
        /// 行业圈子讨论新增
        /// </summary>
        /// <param name="topicInfo">讨论对象</param>
        /// <param name="noticeUserView">提醒人对象</param>
        /// <returns></returns>
        bool Insert(TopicInfo topicInfo, NoticeUserView noticeUserView = null);
        /// <summary>
        /// 行业圈子讨论更新
        /// </summary>
        /// <param name="topicInfo">讨论对象</param>
        /// <param name="noticeUserView">提醒人对象</param>
        /// <returns></returns>
        bool Update(TopicInfo topicInfo, NoticeUserView noticeUserView = null);
        /// <summary>
        /// 基于讨论主题进行搜索
        /// </summary>
        /// <param name="searchView">搜索视图</param>
        /// <param name="page">分页,默认不分页</param>
        /// <returns>讨论主题以及信息列表</returns>
        DataTable Search(TopicSearchView searchView, MiicPage page);
        /// <summary>
        /// 基于讨论主题进行搜索数
        /// </summary>
        /// <param name="searchView">搜索视图</param>
        /// <returns>讨论主题以及信息列表数</returns>
        int GetSearchCount(TopicSearchView searchView);
        /// <summary>
        /// 根据讨论ID获取所有message信息
        /// </summary>
        /// <param name="topicID">讨论ID</param>
        /// <returns>所有message信息</returns>
        DataTable GetMessageListByTopicID(string topicID, MiicPage page = null);
        /// <summary>
        /// 根据讨论ID获取messagecount
        /// </summary>
        /// <param name="topicID">讨论ID</param>
        /// <returns>messagecount</returns>
        int GetMessageCountByTopicID(string topicID);
        /// <summary>
        /// 是否有行业圈子讨论的主题
        /// </summary>
        /// <param name="groupID">行业圈子ID</param>
        /// <returns>Yes/No</returns>
        bool HasCommunityTopic(string communityID);
    }
}
