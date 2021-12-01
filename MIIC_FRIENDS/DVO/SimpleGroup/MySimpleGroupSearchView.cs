using Miic.Base;
using Miic.Friends.General.SimpleGroup;
using Miic.MiicException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.SimpleGroup
{
    public class MySimpleGroupSearchView : GeneralSimpleGroupSearchView
    {
        public string UserID
        {
            get
            {
                return base.userID;
            }
        }

        public MySimpleGroupSearchView()
        {
            Cookie cookie = new Cookie();
            string message = string.Empty;
            base.userID = cookie.GetCookie("SNS_ID", out message);
            if (string.IsNullOrEmpty(this.UserID))
            {
                throw new MiicCookieArgumentNullException("UserID不能为空，Cookie失效");
            }
        }


    }
}
