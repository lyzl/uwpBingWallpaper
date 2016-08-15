using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using BingWallpaper.Model;
using Windows.Storage;
using System.Diagnostics;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BingWallpaper
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        public List<SettingPageItem> settingPageItems = new List<SettingPageItem>();
        public SettingPage()
        {
            this.InitializeComponent();
            //初始化设置文件结构,如将来文件结构复杂,则改用递归遍历类
            if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey("settings"))
            {
                ApplicationData.Current.LocalSettings.CreateContainer("settings", ApplicationDataCreateDisposition.Always);
                if (!ApplicationData.Current.LocalSettings.Containers["settings"].Containers.ContainsKey("toggle"))
                {
                    ApplicationData.Current.LocalSettings.Containers["settings"].CreateContainer("toggle", ApplicationDataCreateDisposition.Always);
                }
            }
            settingPageItems = SettingPageItem.GetSettingItems();
        }

        private async void DaliyUpdateSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch.IsOn)
            {
                var access = await BackgroundExecutionManager.RequestAccessAsync();
                if (access != BackgroundAccessStatus.Denied)
                {
                    bool taskRegistered = BackgroundTaskRegistration.AllTasks.Any(p => p.Value.Name == "BingWallpaperDailyUpdate");
                    if (!taskRegistered)
                    {
                        var builder = new BackgroundTaskBuilder();
                        builder.Name = "BingWallpaperDailyUpdate";
                        builder.TaskEntryPoint = "BackgroundTask.DailyUpdateBackgroundTask";
                        builder.SetTrigger(new MaintenanceTrigger(120, true));
                        BackgroundTaskRegistration task = builder.Register();
                    }
                }
                else
                {
                    await new MessageDialog("后台任务被禁止了").ShowAsync();
                    toggleSwitch.IsOn = false;
                }
            }
            else
            {
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == "BingWallpaperDailyUpdate")
                    {
                        task.Value.Unregister(true);
                    }
                }
            }
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
