using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Json;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Storage;
using Windows.System.UserProfile;
using Windows.UI.Notifications;
using BackgroundTask;

namespace BackgroundTask
{
    public sealed class DailyUpdateBackgroundTask:IBackgroundTask
    {
        const bool debug = false;
        BackgroundTaskDeferral defferal;
        WallPapers dailyWallpaper;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            defferal = taskInstance.GetDeferral();
            dailyWallpaper = await WallpaperProxy.GetWallpaper(1, 1);
            if (!checkAvaliableNewWallpaper() &&(!debug))
            {
                defferal.Complete();
                return;
            }
            var localFolder = ApplicationData.Current.LocalFolder.GetFolderAsync("wallpapers");
            var localToggleSettings = ApplicationData.Current.LocalSettings.Containers["settings"].Containers["toggle"];
            var imageFile = await WallpaperProxy.SaveWallPaper(dailyWallpaper.images[0]);
            if ((bool)localToggleSettings.Values["autoSetToWallpaper"])
            {
                await UserProfilePersonalizationSettings.Current.TrySetWallpaperImageAsync(imageFile as StorageFile);
                //更新桌面壁纸
            }

            if ((bool)localToggleSettings.Values["autoSetToLockScreen"])
            {
                await UserProfilePersonalizationSettings.Current.TrySetLockScreenImageAsync(imageFile as StorageFile);
                //更新锁屏壁纸
            }

            if ((bool)localToggleSettings.Values["popToast"])
            {
                PopToast(imageFile.Path.ToString(), dailyWallpaper.images[0].copyright);
                //推送通知
            }
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
            defferal.Complete();
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            Debug.WriteLine(reason);
        }

        private void PopToast(string imagePath, string copyright)
        {
            string xml = $@"
                <toast scenario='reminder'>
                  <visual>
                    <binding template='ToastGeneric'>
                      <text>今日必应图纸</text>
                      <text>{copyright}</text>
                      <image placement='inline' src='file:///{imagePath}'/>
                    </binding>
                  </visual>
                </toast>";
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            var toast = new ToastNotification(doc);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private bool checkAvaliableNewWallpaper()
        {
            bool rt;
            var builder = new BackgroundTaskBuilder();
            builder.Name = "BingWallpaperDailyUpdate";
            builder.TaskEntryPoint = "BackgroundTask.DailyUpdateBackgroundTask";
            foreach (var item in BackgroundTaskRegistration.AllTasks)
            {
                if (item.Value.Name == "BingWallpaperDailyUpdate")
                {
                    item.Value.Unregister(true);
                }
            }
            if (File.Exists(string.Format("{0}//wallpapersInfo//{1}.txt", ApplicationData.Current.LocalFolder.Path, dailyWallpaper.images[0].fullstartdate)))
            {
                //存在文件,代表没有新的壁纸
                builder.SetTrigger(new MaintenanceTrigger(120, true));
                rt = false;
            }
            else
            {
                //不存在文件,代表有新的壁纸
                builder.SetTrigger(new MaintenanceTrigger(1200, true));
                rt = true;
            }
            BackgroundTaskRegistration task = builder.Register();
            return rt;
        }
    }
}
