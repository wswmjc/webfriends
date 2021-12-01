using Miic.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Moments
{
    public interface IBehavior<T> : ICommon<T> where T : class
    {
    }
}
