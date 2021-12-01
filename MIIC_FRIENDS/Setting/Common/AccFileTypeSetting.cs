using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Common.Setting
{
    public enum AccFileTypeSetting
    {
        /// <summary>
        /// 图片
        /// </summary>
        [Description("图片")]
        Photo = 0,
        /// <summary>
        /// Word文件
        /// </summary>
        [Description("Word")]
        Word = 1,
        /// <summary>
        /// Excel文件
        /// </summary>
        [Description("Excel")]
        Excel=2,
        /// <summary>
        /// 幻灯片文件
        /// </summary>
        [Description("PowerPoint")]
        PowerPoint=3,
        /// <summary>
        /// PDF文件
        /// </summary>
        [Description("Pdf")]
        Pdf=4,
        /// <summary>
        /// 文本文件
        /// </summary>
        [Description("文本")]
        Text=5,
        /// <summary>
        /// 附件
        /// </summary>
        [Description("附件")]
        Accessory=6,
        /// <summary>
        /// Xml
        /// </summary>
        [Description("Xml")]
        Xml=7
    }
}
