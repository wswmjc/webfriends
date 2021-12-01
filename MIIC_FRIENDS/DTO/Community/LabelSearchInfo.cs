using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Community
{
    [MiicTable(MiicStorageName = "COMMUNITY_LABEL_SEARCH_INFO", Description = "行业圈子标签查询视图")]
    public class LabelSearchInfo
    {
        [MiicField(MiicStorageName = "PUBLISH_ID", MiicDbType = DbType.String, Description = "发布ID")]
        public string PublishID { get; set; }
     
        [MiicField(MiicStorageName = "COMMUNITY_ID", MiicDbType = DbType.String, Description = "圈子ID")]
        public string CommunityID { get; set; }
         [MiicField(MiicStorageName = "LABEL_ID",  MiicDbType = DbType.String, Description = "标签名称")]
        public string LabelID { get; set; }
        [MiicField(MiicStorageName = "LABEL_NAME",  MiicDbType = DbType.String, Description = "标签名称")]
        public string LabelName { get; set; }
        [MiicField(MiicStorageName = "USER_ID",  MiicDbType = DbType.String, Description = "发布者ID")]
        public string UserID { get; set; }
        [MiicField(MiicStorageName = "SOCIAL_CODE",  MiicDbType = DbType.String, Description = "发布者SocialCode")]
        public string SocialCode { get; set; }
        [MiicField(MiicStorageName = "USER_NAME", MiicDbType = DbType.String, Description = "发布者")]
        public string UserName { get; set; }
        [MiicField(MiicStorageName = "USER_TYPE", MiicDbType = DbType.String, Description = "发布者类型")]
        public string UserType { get; set; }
        [MiicField(MiicStorageName = "USER_URL",MiicDbType = DbType.String, Description = "发布者URL")]
        public string UserUrl { get; set; }
       
    }
}
