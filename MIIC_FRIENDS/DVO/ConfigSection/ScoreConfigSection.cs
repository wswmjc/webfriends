using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Common.ConfigSection
{

    public class ScoreConfigSection : ConfigurationSection
    {
        public ScoreConfigSection() { }
        [ConfigurationProperty("PublishScore", IsRequired = true, DefaultValue = "1")]
        public int PublishScore
        {
            get { return int.Parse(this["PublishScore"].ToString()); }
            set { this["PublishScore"] = value; }
        }
        [ConfigurationProperty("CreateOrgScore", IsRequired = true, DefaultValue = "1")]
        public int CreateOrgScore
        {
            get { return int.Parse(this["CreateOrgScore"].ToString()); }
            set { this["CreateOrgScore"] = value; }
        }
        [ConfigurationProperty("BehaviorScore", IsRequired = true, DefaultValue = "1")]
        public int BehaviorScore
        {
            get { return int.Parse(this["BehaviorScore"].ToString()); }
            set { this["BehaviorScore"] = value; }
        }
        [ConfigurationProperty("CommentScore", IsRequired = true, DefaultValue = "1")]
        public int CommentScore
        {
            get { return int.Parse(this["CommentScore"].ToString()); }
            set { this["CommentScore"] = value; }
        }
    }
}
