using Miic.Friends.Common.ConfigSection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace Miic.Friends.Common
{
    public static class ScoreConfig
    {
        public static readonly string ServiceID = WebConfigurationManager.AppSettings["ServiceID"].ToString();
        public static readonly ScoreConfigSection Score = (ScoreConfigSection)WebConfigurationManager.GetSection("ScoreConfigSection");
    }
}
