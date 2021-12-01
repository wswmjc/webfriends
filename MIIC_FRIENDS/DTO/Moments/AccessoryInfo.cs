using Miic.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Moments
{
    [MiicTable(MiicStorageName = "MOMENTS_ACC_INFO", Description = "朋友圈发布信息附件表")]
    public partial class AccessoryInfo
    {
        [MiicField(MiicStorageName = "ID", IsNotNull = true, IsPrimaryKey = true, MiicDbType = DbType.String, Description = "唯一码")]
        public string ID { get; set; }
        [MiicField(MiicStorageName = "PUBLISH_ID", IsNotNull = true,  MiicDbType = DbType.String, Description = "发布信息ID")]
        public string PublishID { get; set; }
        [MiicField(MiicStorageName = "FILE_NAME", IsNotNull = true, MiicDbType = DbType.String, Description = "文件名")]
        public string FileName { get; set; }
        [MiicField(MiicStorageName = "FILE_PATH", IsNotNull = true, MiicDbType = DbType.String, Description = "文件路径")]
        public string FilePath { get; set; }
        [MiicField(MiicStorageName = "UPLOAD_TIME", IsNotNull = true,  MiicDbType = DbType.String, Description = "上传时间")]
        public DateTime? UploadTime { get; set; }
        [MiicField(MiicStorageName = "FILE_TYPE", IsNotNull = true, MiicDbType = DbType.String, Description = "文件类型")]
        public string FileType { get; set; }
    }
}
