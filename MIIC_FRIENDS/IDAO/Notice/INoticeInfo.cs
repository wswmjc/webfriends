using Miic.Base;
using Miic.DB.SqlObject;
using Miic.Friends.Common.Setting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Notice
{
    public  interface INoticeInfo<T>:ICommon<T>
    {
        /// <summary>
        /// 获取@/消息我的相关通知列表
        /// </summary>
        /// <param name="myNoticeView">我的通知视图</param>
        /// <param name="page">分页，默认不分页</param>
        /// <returns>@通知列表</returns>
        DataTable GetMyNoticeInfoList(MyNoticeView myNoticeView,MiicPage page=null);
        /// <summary>
        /// 获取@/消息我的相关通知列表数
        /// </summary>
        /// <param name="myNoticeView">我的通知视图</param>
        /// <returns>@通知列表数</returns>
        int GetMyNoticeInfoCount(MyNoticeView myNoticeView);
        /// <summary>
        /// 批量全部设置已读
        /// </summary>
        /// <param name="noticerID">被通知人</param>
        /// <param name="businessType">来源</param>
        /// <returns></returns>
        bool ReadAllNotice(string noticerID, BusinessTypeSetting type);
    }
}
