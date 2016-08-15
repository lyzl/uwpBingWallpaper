using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BingWallpaper.Model
{
    public class SettingPageItem
    {
        private bool _settingValue;

        public string descrption { get; set; }
        public string settingKey { get; set; }
        public bool settingValue
        {
            get { return _settingValue; }
            set {
                _settingValue = value;
                ApplicationData.Current.LocalSettings.Containers["settings"].Containers["toggle"].Values[settingKey] = value;
            }
        }

        SettingPageItem(string descrption, string key)
        {
            this.descrption = descrption;
            this.settingKey = key;
            if (ApplicationData.Current.LocalSettings.Containers["settings"].Containers["toggle"].Values.ContainsKey(key))
            {
                _settingValue = (bool)ApplicationData.Current.LocalSettings.Containers["settings"].Containers["toggle"].Values[key];
            }
            else
            {
                ApplicationData.Current.LocalSettings.Containers["settings"].Containers["toggle"].Values[key] = false;
                settingValue = false;
            }
        }

        public static List<SettingPageItem> GetSettingItems()
        {
            var items = new List<SettingPageItem>() {
                new SettingPageItem("每日自动获取壁纸","dailyUpadate"),
                new SettingPageItem("自动设置为桌面壁纸","autoSetToWallpaper"),
                new SettingPageItem("自动设置为锁屏壁纸", "autoSetToLockScreen"),
                new SettingPageItem("通知获得到的壁纸","popToast")
            };
            return items;
        }

    }

    
}
