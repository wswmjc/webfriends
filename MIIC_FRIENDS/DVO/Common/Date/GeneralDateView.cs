using Miic.DB.SqlObject;
using Miic.Friends.Moments;

namespace Miic.Friends.Common
{
    public abstract class GeneralDateView
    {
        /// <summary>
        /// 年份
        /// </summary>
        public string Year { get; set; }
        /// <summary>
        /// 月份
        /// </summary>
        public string Month { get; set; }
        /// <summary>
        /// 查询者ID
        /// </summary>
        protected internal string userID;
        public abstract MiicConditionCollections visitor(PublishInfoDao publishInfoDao);
      
    }
}
