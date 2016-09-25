using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BingWallpaper.Model
{
    [DataContract]
    public sealed class Wallpaper
    {
        [DataMember]
        public string startdate { get; set; }
        [DataMember]
        public string fullstartdate { get; set; }
        [DataMember]
        public string enddate { get; set; }
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public string urlbase { get; set; }
        [DataMember]
        public string copyright { get; set; }
        [DataMember]
        public string copyrightlink { get; set; }
        [DataMember]
        public bool wp { get; set; }
        [DataMember]
        public string hsh { get; set; }
        [DataMember]
        public int drk { get; set; }
        [DataMember]
        public int top { get; set; }
        [DataMember]
        public int bot { get; set; }
        [DataMember]
        public List<object> hs { get; set; }
    }

    [DataContract]
    public sealed class Tooltips
    {
        [DataMember]
        public string loading { get; set; }
        [DataMember]
        public string previous { get; set; }
        [DataMember]
        public string next { get; set; }
        [DataMember]
        public string walle { get; set; }
        [DataMember]
        public string walls { get; set; }
    }

    [DataContract]
    public sealed class Wallpapers
    {
        [DataMember]
        public List<Wallpaper> images { get; set; }
        [DataMember]
        public Tooltips tooltips { get; set; }
    }

}
