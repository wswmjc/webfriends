using Miic.Base;
using Miic.Base.Setting;
using Miic.MiicException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Moments
{
    public class CommentSearchView
    {
        /// <summary>
        /// 查询者ID
        /// </summary>
        protected internal string userID;
        /// <summary>
        /// 查询者ID
        /// </summary>
        public string UserID
        {
            get
            {
                return this.userID;
            }
        }
        /// <summary>
        /// 信息ID
        /// </summary>
        public string PublishID { get; set; }
        /// <summary>
        /// 是否屏蔽非好友评论
        /// </summary>
        public MiicYesNoSetting WithAddress { get; set; }

        public CommentSearchView()
        {
            //默认展示所有评论
            this.WithAddress = MiicYesNoSetting.No;

            Cookie cookie = new Cookie();
            string message = string.Empty;
            this.userID = cookie.GetCookie("SNS_ID", out message);
            if (string.IsNullOrEmpty(this.UserID))
            {
                throw new MiicCookieArgumentNullException("UserID不能为空，Cookie失效");
            }
        }
    }
}
