using Miic.Base;
using Miic.Common;
using Miic.DB.SqlObject;
using System.Collections.Generic;
using System.Data;

namespace Miic.Friends.Community
{
    public interface ILabelInfo:ICommon<LabelInfo>
    {
        /// <summary>
        /// 设置标签的有效性
        /// </summary>
        /// <param name="validView">有效性视图</param>
        /// <returns>Yes/No</returns>
        bool SetValid(ValidView validView);
        /// <summary>
        /// 批量禁用标签
        /// </summary>
        /// <param name="labelIDs">标签ID组</param>
        /// <returns>yes/no</returns>
        bool DisableLabels(List<string> labelIDs);
        /// <summary>
        /// 根据圈子获取有效标签集合
        /// </summary>
        /// <param name="communityID">圈子ID</param>
        /// <returns>标签集合</returns>
        DataTable GetLabelInfosByCommunityID(string communityID);
        /// <summary>
        /// 检测标签名称的唯一性
        /// </summary>
        /// <param name="communityID">圈子ID</param>
        /// <param name="labelName">标签名称</param>
        /// <returns>不存在true 存在false</returns>
        bool UniqueLabel(string communityID, string labelName);
        /// <summary>
        /// 检测标签是否被使用
        /// </summary>
        /// <param name="labelID">标签ID</param>
        /// <returns>使用true 未使用false</returns>
        bool HasUsed(string labelID);
        /// <summary>
        /// 删除Label信息(管理员使用)
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>Yes/No</returns>
        /// <remarks>包括Relation的内容，级联</remarks>
        bool DeepDelete(string id);
        /// <summary>
        /// 查询行业圈子标签主题列表
        /// </summary>
        /// <param name="keywordView">关键字视图</param>
        /// <param name="page">分页，默认：不分页</param>
        /// <returns>信息列表</returns>
        DataTable Search(NoPersonKeywordView keywordView, MiicPage page);
        /// <summary>
        /// 获取行业圈子标签主题列表数
        /// </summary>
        /// <param name="keywordView">关键字视图</param>
        /// <returns>信息列表数</returns>
        int GetSearchCount(NoPersonKeywordView keywordView);
        /// <summary>
        /// 根据标签ID获取指定标签集合
        /// </summary>
        /// <param name="labelIDs">标签ID数组</param>
        /// <returns>标签集合</returns>
        DataTable GetLabelListWithIDs(List<string> labelIDs);
    }
}
