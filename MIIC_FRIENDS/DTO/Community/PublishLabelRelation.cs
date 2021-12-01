using Miic.Attribute;
using System.Data;

namespace Miic.Friends.Community
{
    [MiicTable(MiicStorageName = "COMMUNITY_PUBLISH_LABEL_RELATION", Description = "圈子信息标签关系表")]
    public class PublishLabelRelation
    {
        [MiicField(MiicStorageName = "PUBLISH_ID", IsNotNull = true, IsPrimaryKey = true, MiicDbType = DbType.String, Description = "圈子信息ID")]
        public string PublishID { get; set; }
        [MiicField(MiicStorageName = "LABEL_ID", IsNotNull = true, IsPrimaryKey = true, MiicDbType = DbType.String, Description = "标签ID")]
        public string LabelID { get; set; }
        [MiicField(MiicStorageName = "COMMUNITY_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "圈子ID")]
        public string CommunityID { get; set; }
        [MiicField(MiicStorageName = "PUBLISH_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "圈子信息标题")]
        public string PublishName { get; set; }
        [MiicField(MiicStorageName = "LABEL_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "标签名称")]
        public string LabelName { get; set; }
         [MiicField(MiicStorageName = "VALID", IsNotNull = true, MiicDbType = DbType.String, Description = "有效性")]
        public string Valid { get; set; }
        [MiicField(MiicStorageName = "SORT_NO", IsNotNull = true, IsIdentification = true, MiicDbType = DbType.Int32, Description = "排序")]
        public int? SortNo { get; set; }
    }
}
