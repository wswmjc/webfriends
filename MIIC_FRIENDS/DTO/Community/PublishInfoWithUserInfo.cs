using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Community
{
    [MiicTable(MiicStorageName = "COMMUNITY_PUBLISH_INFO_WITH_USER", Description = "圈子发布(含用户个人信息)视图")]
    public class PublishInfoWithUserInfo
    {
        [MiicField(MiicStorageName = "ID",  MiicDbType = DbType.String, Description = "唯一码")]
        public string ID { get; set; }
        [MiicField(MiicStorageName = "PUBLISH_TITLE", MiicDbType = DbType.String, Description = "标题")]
        public string Title { get; set; }
        [MiicField(MiicStorageName = "PUBLISH_CONTENT",  MiicDbType = DbType.String, Description = "内容")]
        public string Content { get; set; }
        [MiicField(MiicStorageName = "CREATER_ID",  MiicDbType = DbType.String, Description = "创建者ID")]
        public string CreaterID { get; set; }
        [MiicField(MiicStorageName = "CREATER_NAME",  MiicDbType = DbType.String, Description = "创建者名称")]
        public string CreaterName { get; set; }
        [MiicField(MiicStorageName = "CREATE_TIME",  MiicDbType = DbType.DateTime, Description = "创建时间")]
        public DateTime? CreateTime { get; set; }
        [MiicField(MiicStorageName = "PUBLISH_TYPE",  MiicDbType = DbType.String, Description = " 发布类型")]
        public string PublishType { get; set; }
        [MiicField(MiicStorageName = "PUBLISH_TIME", MiicDbType = DbType.DateTime, Description = "发布时间")]
        public DateTime? PublishTime { get; set; }
        [MiicField(MiicStorageName = "HAS_ACC",  MiicDbType = DbType.String, Description = "是否含有附件")]
        public string HasAcc { get; set; }
        [MiicField(MiicStorageName = "EDIT_STATUS", MiicDbType = DbType.String, Description = "编辑状态")]
        public string EditStatus { get; set; }
        [MiicField(MiicStorageName = "UPDATE_TIME", MiicDbType = DbType.DateTime, Description = "更新时间")]
        public DateTime? UpdateTime { get; set; }
        [MiicField(MiicStorageName = "BROWSE_NUM",  MiicDbType = DbType.Int32, Description = "浏览总数")]
        public int? BrowseNum { get; set; }
        [MiicField(MiicStorageName = "PRAISE_NUM",  MiicDbType = DbType.Int32, Description = "点赞总数")]
        public int? PraiseNum { get; set; }
        [MiicField(MiicStorageName = "TREAD_NUM",  MiicDbType = DbType.Int32, Description = "点踩总数")]
        public int? TreadNum { get; set; }
        [MiicField(MiicStorageName = "TRANSMIT_NUM",  MiicDbType = DbType.Int32, Description = "转发总数")]
        public int? TransmitNum { get; set; }
        [MiicField(MiicStorageName = "REPORT_NUM",  MiicDbType = DbType.Int32, Description = "举报总数")]
        public int? ReportNum { get; set; }
        [MiicField(MiicStorageName = "COMMENT_NUM",MiicDbType = DbType.Int32, Description = "评论总数")]
        public int? CommentNum { get; set; }
        [MiicField(MiicStorageName = "COLLECT_NUM",  MiicDbType = DbType.Int32, Description = "被收藏数")]
        public int? CollectNum { get; set; }
        [MiicField(MiicStorageName = "SORT_NO",  MiicDbType = DbType.Int32, Description = "排序")]
        public int? SortNo { get; set; }
        [MiicField(MiicStorageName = "MICRO_USER_URL",  MiicDbType = DbType.String, Description = "用户头像")]
        public string MicroUserUrl { get; set; }
        [MiicField(MiicStorageName = "ORG_NAME",  MiicDbType = DbType.String, Description = "企业名称")]
        public string OrgName { get; set; }
        [MiicField(MiicStorageName = "USER_NAME", MiicDbType = DbType.String, Description = "姓名")]
        public string UserName { get; set; }
        [MiicField(MiicStorageName = "USER_TYPE", MiicDbType = DbType.String, Description = "用户类型")]
        public string UserType { get; set; }
    }
}
