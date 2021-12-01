using Miic.Attribute;
using System.Data;

namespace Miic.Friends.Community
{
    [MiicTable(MiicStorageName = "COMMUNITY_LABEL_INFO", Description = "圈子标签表")]
    public partial class LabelInfo : Miic.Friends.General.SimpleGroup.GeneralSimpleProjectInfo
    {
        [MiicField(MiicStorageName = "COMMUNITY_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "圈子ID")]
        public string CommunityID { get; set; }
        [MiicField(MiicStorageName = "LABEL_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "标签名称")]
        public string LabelName { get; set; }
    }
}
