using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Moments
{
    [MiicTable(MiicStorageName = "MOMENTS_PUBLISH_INFO", Description = "朋友圈发布表")]
    public partial class PublishInfo
    {
        [MiicField(MiicStorageName = "ID", IsNotNull = true, IsPrimaryKey = true, MiicDbType = DbType.String, Description = "唯一码")]
        public string ID { get; set; }
        [MiicField(MiicStorageName = "MOMENTS_TITLE", MiicDbType = DbType.String, Description = "标题")]
        public string Title { get; set; }
        [MiicField(MiicStorageName = "MOMENTS_CONTENT", IsNotNull = true, MiicDbType = DbType.String, Description = "内容")]
        public string Content { get; set; }
        [MiicField(MiicStorageName = "CREATER_ID", IsNotNull = true, MiicDbType = DbType.String, Description = "创建者ID")]
        public string CreaterID { get; set; }
        [MiicField(MiicStorageName = "CREATER_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "创建者名称")]
        public string CreaterName { get; set; }
        [MiicField(MiicStorageName = "CREATE_TIME", IsNotNull = true, MiicDbType = DbType.DateTime, Description = "创建时间")]
        public DateTime? CreateTime { get; set; }
        [MiicField(MiicStorageName = "PUBLISH_TYPE", IsNotNull = true, MiicDbType = DbType.String, Description = "发布类型")]
        public string PublishType { get; set; }
        [MiicField(MiicStorageName = "PUBLISH_TIME", MiicDbType = DbType.DateTime, Description = "发布时间")]
        public DateTime? PublishTime { get; set; }
        [MiicField(MiicStorageName = "HAS_ACC", IsNotNull = true, MiicDbType = DbType.String, Description = "是否含有附件")]
        public string HasAcc { get; set; }
        [MiicField(MiicStorageName = "EDIT_STATUS", IsNotNull = true, MiicDbType = DbType.String, Description = "编辑状态")]
        public string EditStatus { get; set; }
       
        [MiicField(MiicStorageName = "UPDATE_TIME", MiicDbType = DbType.DateTime, Description = "更新时间")]
        public DateTime? UpdateTime { get; set; }
        [MiicField(MiicStorageName = "BROWSE_NUM", IsNotNull = true, MiicDbType = DbType.Int32, Description = "浏览总数")]
        public int? BrowseNum { get; set; }
        [MiicField(MiicStorageName = "PRAISE_NUM", IsNotNull = true, MiicDbType = DbType.Int32, Description = "点赞总数")]
        public int? PraiseNum { get; set; }
        [MiicField(MiicStorageName = "TREAD_NUM", IsNotNull = true, MiicDbType = DbType.Int32, Description = "点踩总数")]
        public int? TreadNum { get; set; }
        [MiicField(MiicStorageName = "TRANSMIT_NUM", IsNotNull = true, MiicDbType = DbType.Int32, Description = "转发总数")]
        public int? TransmitNum { get; set; }
        [MiicField(MiicStorageName = "REPORT_NUM", IsNotNull = true, MiicDbType = DbType.Int32, Description = "举报总数")]
        public int? ReportNum { get; set; }
        [MiicField(MiicStorageName = "COMMENT_NUM", IsNotNull = true, MiicDbType = DbType.Int32, Description = "评论总数")]
        public int? CommentNum { get; set; }
        [MiicField(MiicStorageName = "COLLECT_NUM", IsNotNull = true, MiicDbType = DbType.Int32, Description = "被收藏数")]
        public int? CollectNum { get; set; }
        [MiicField(MiicStorageName = "SORT_NO", IsIdentification = true, IsNotNull = true, MiicDbType = DbType.Int32, Description = "排序")]
        public int? SortNo { get; set; }

    }

}
