using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.AddressBook
{
    public partial class SetCanSeeMeView
    {
        /// <summary>
        /// 通讯录ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 是否能看我
        /// </summary>
        public bool CanSeeMe { get; set; }
    }
}
