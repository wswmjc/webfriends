using Miic.Base;
using Miic.Base.Setting;
using Miic.DB.Setting;
using Miic.DB.SqlObject;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Notice
{
    public class UnreadNoticeView
    {
        /// <summary>
        /// 通知类型
        /// </summary>
        public string NoticeType { get; set; }
        /// <summary>
        /// 通知来源
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 信息发布ID
        /// </summary>
        public string PublishID { get; set; }
        /// <summary>
        /// 发布者ID
        /// </summary>
        public string PublisherID { get; set; }
        /// <summary>
        /// 通知者ID
        /// </summary>
        public List<string> NoticerID { get; set; }

        public UnreadNoticeView()
        {

        }
        public MiicConditionCollections visitor(NoticeInfoDao noticeInfoDao)
        {
            MiicConditionCollections result = new MiicConditionCollections(MiicDBLogicSetting.No);
            MiicCondition publishIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<NoticeInfo, string>(o => o.PublishID),
                this.PublishID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(MiicDBLogicSetting.No, publishIDCondition));
            MiicCondition publisherIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<NoticeInfo, string>(o => o.PublisherID),
                this.PublisherID,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(publisherIDCondition));
            MiicCondition noticerIDCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<NoticeInfo, string>(o => o.NoticerID),
                this.NoticerID,
                DbType.String,
                MiicDBOperatorSetting.In);
            result.Add(new MiicConditionLeaf(noticerIDCondition));
            MiicCondition readStatusCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<NoticeInfo, string>(o => o.ReadStatus),
                 ((int)MiicReadStatusSetting.UnRead).ToString(),
                DbType.String,
                MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(readStatusCondition));
            MiicCondition sourceCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<NoticeInfo, string>(o => o.Source),
                this.Source,
                DbType.String,
                MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(publisherIDCondition));
            MiicCondition noticeTypeCondition = new MiicCondition(Config.Attribute.GetSqlColumnNameByPropertyName<NoticeInfo, string>(o => o.NoticeType),
               this.NoticeType,
               DbType.String,
               MiicDBOperatorSetting.Equal);
            result.Add(new MiicConditionLeaf(noticeTypeCondition));
            return result;
        }
    }
}
