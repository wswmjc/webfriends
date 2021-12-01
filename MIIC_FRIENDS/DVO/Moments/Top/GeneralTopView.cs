using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Miic.Friends.Common.Setting;
using Miic.DB.SqlObject;

namespace Miic.Friends.Moments
{
   public abstract class GeneralTopView
    {
       /// <summary>
       /// top数量
       /// </summary>
       public int Top { get; set; }
       /// <summary>
       /// 查询页面
       /// </summary>
       public PublishInfoBelongSetting Belong { get; set; }
       /// <summary>
       /// 查询ID
       /// </summary>
       protected internal string userID;
       /// <summary>
       /// 访问接口
       /// </summary>
       /// <param name="publishInfoDao"></param>
       /// <returns></returns>
       public abstract MiicConditionCollections visitor(PublishInfoDao publishInfoDao);
    }
}
