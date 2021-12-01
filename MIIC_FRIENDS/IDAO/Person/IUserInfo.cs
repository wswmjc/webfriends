using Miic.BaseStruct;
using Miic.DB.SqlObject;
using Miic.Friends.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.User
{
    public interface IUserInfo
    {
        /// <summary>
        /// 根据用户ID获取某人的统计信息，包括：发布数、所在行业圈子数、所在讨论组数
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns>统计信息列表</returns>
        List<MiicKeyValue> GetPersonStatisticsCount(string userID);
        /// <summary>
        /// 搜索用户
        /// </summary>
        /// <param name="keywordView">关键字视图</param>
        /// <param name="page">分页</param>
        /// <returns>用户信息列表</returns>
        DataTable Search(KeywordView keywordView,MiicPage page=null);
        int GetSearchCount(KeywordView keywordView);
    }
}
