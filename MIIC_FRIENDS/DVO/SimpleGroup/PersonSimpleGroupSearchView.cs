using Miic.Friends.General.SimpleGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.SimpleGroup
{
    public class PersonSimpleGroupSearchView : GeneralSimpleGroupSearchView
    {
        public string UserID
        {
            get
            {
                return base.userID;
            }
            set 
            {
                base.userID = value;
            }
        }
        public PersonSimpleGroupSearchView() 
        {

        }
    }
}
