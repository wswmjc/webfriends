using Miic.Base;
using Miic.Base.Setting;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using System;
using System.Collections.Generic;
using System.Data;

namespace Miic.Friends.Common
{
   public  class PersonKeywordView:KeywordView
    {

       /// <summary>
       /// 用户ID
       /// </summary>
       public string UserID
       {
           get { return this.userID; }
           set { this.userID = value; }
       }
     
      
     
   }
}
