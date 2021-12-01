using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.AddressBook
{
    public partial class SetOftenUsedView
    {
        /// <summary>
        /// 通讯录ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 是否经常使用
        /// </summary>
        public bool OftenUsed { get; set; }
    }
}
