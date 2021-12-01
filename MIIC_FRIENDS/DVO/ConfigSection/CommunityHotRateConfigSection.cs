using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miic.Friends.Community
{
    public class CommunityHotRateConfigSection : ConfigurationSection
    {
        public CommunityHotRateConfigSection()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("MemberRatio", IsRequired = true, DefaultValue = "1")]
        public float MemberRatio
        {
            get { return float.Parse(this["MemberRatio"].ToString()); }
            set { this["MemberRatio"] = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [ConfigurationProperty("TopicRatio", IsRequired = true, DefaultValue = "1")]
        public float TopicRatio
        {
            get { return float.Parse(this["TopicRatio"].ToString()); }
            set { this["TopicRatio"] = value; }
        }

        [ConfigurationProperty("TopicMessageRatio", IsRequired = true, DefaultValue = "1")]
        public float TopicMessageRatio
        {
            get { return float.Parse(this["TopicMessageRatio"].ToString()); }
            set { this["TopicMessageRatio"] = value; }
        }
        [ConfigurationProperty("PublishRatio", IsRequired = true, DefaultValue = "1")]
        public float PublishRatio
        {
            get { return float.Parse(this["PublishRatio"].ToString()); }
            set { this["PublishRatio"] = value; }
        }
        [ConfigurationProperty("PublishPartakeRatio", IsRequired = true, DefaultValue = "1")]
        public float PublishPartakeRatio 
        {
            get { return float.Parse(this["PublishPartakeRatio"].ToString()); }
            set { this["PublishPartakeRatio"] = value; }
        }
    }
}
