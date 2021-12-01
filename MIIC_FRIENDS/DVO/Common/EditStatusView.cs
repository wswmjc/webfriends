using Miic.Base.Setting;

namespace Miic.Friends.Common
{
    public partial class EditStatusView
    {
        /// <summary>
        /// ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 上下线状态
        /// </summary>
        public MiicYesNoSetting EditStatus { get; set; }
    }
}
