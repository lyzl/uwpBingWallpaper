using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingWallpaper.Model
{
    public class NavBarMenuItem
    {
        public string icon { get; set; }
        public string descrption { get; set; }
        public Type destPage { get; set; }
        NavBarMenuItem(string icon, string descrption,Type destPage)
        {
            this.icon = icon;
            this.descrption = descrption;
            this.destPage = destPage;
        }

        public static List<NavBarMenuItem> GetNavBarMenuItems()
        {
            var items = new List<NavBarMenuItem> {
                new NavBarMenuItem("\xEB9F", "今日图片", typeof(SinglePictureView)),
                new NavBarMenuItem("\xE2AD", "往日图片", typeof(MultiPicturesView))};
            return items;
        }
    }

}
