using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources.Core;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Storage;
using System.IO;
using Windows.System.UserProfile;

namespace VoiceCommandService
{
    public sealed class BingWallpaperVoiceCommandService : IBackgroundTask
    {
        const bool debug = true;
        VoiceCommandServiceConnection voiceServiceConnection;
        BackgroundTaskDeferral deferral;
        ResourceMap cortanaResourceMap;
        ResourceContext cortanaContest;
        Wallpapers dailyWallpapers;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += OntaskCancled;
            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            cortanaResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");
            cortanaContest = ResourceContext.GetForViewIndependentUse();

            if (triggerDetails != null && triggerDetails.Name == "BingWallpaperVoiceCommandService")
            {
                try
                {
                    voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);
                    voiceServiceConnection.VoiceCommandCompleted += OnVoiceCommandCompleted;
                    VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();
                    switch (voiceCommand.CommandName)
                    {
                        case "refreshDailyWallpaper":
                            await SendCompletionMessageForRefresh();
                            break;
                        default:
                            break;
                    }

                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }
            }
        }

        private async Task SendCompletionMessageForRefresh()
        {
            var progressScreenString = "正在查找新的壁纸";
            var userProgressMessage = new VoiceCommandUserMessage();
            userProgressMessage.DisplayMessage = userProgressMessage.SpokenMessage = progressScreenString;

            VoiceCommandResponse progressResponse = VoiceCommandResponse.CreateResponse(userProgressMessage);
            await voiceServiceConnection.ReportProgressAsync(progressResponse);

            var userMessage = new VoiceCommandUserMessage();
            var wallpaperContentTitle = new VoiceCommandContentTile();

            dailyWallpapers = await WallpaperProxy.GetWallpaper(1, 1);
            if (checkAvaliableNewWallpaper())
            {
                userMessage.DisplayMessage = "您的壁纸已更新";
                var imageFile = await WallpaperProxy.SaveWallPaper(dailyWallpapers.images[0]);
                userMessage.SpokenMessage = "您的壁纸已更新";
                await UserProfilePersonalizationSettings.Current.TrySetLockScreenImageAsync(imageFile as StorageFile);
                await UserProfilePersonalizationSettings.Current.TrySetWallpaperImageAsync(imageFile as StorageFile);
                wallpaperContentTitle.ContentTileType = VoiceCommandContentTileType.TitleWith280x140IconAndText;
                wallpaperContentTitle.Image = await StorageFile.GetFileFromPathAsync(dailyWallpapers.images[0].url);
                wallpaperContentTitle.TextLine1 = dailyWallpapers.images[0].copyright;
            }
            else
            {
                userMessage.DisplayMessage = "现在的壁纸已经是最新了";
                userMessage.SpokenMessage = "现在的壁纸已经是最新了";
                wallpaperContentTitle.ContentTileType = VoiceCommandContentTileType.TitleOnly;
            }

            wallpaperContentTitle.Title = "每日必应壁纸";

            var successResponse = VoiceCommandResponse.CreateResponse(userMessage);
            await voiceServiceConnection.ReportSuccessAsync(successResponse);
        }

        private bool checkAvaliableNewWallpaper()
        {
            bool rt;

            if (File.Exists(string.Format("{0}//WallpapersInfo//{1}.txt", ApplicationData.Current.LocalFolder.Path, dailyWallpapers.images[0].fullstartdate)))
            {
                //存在文件,代表没有新的壁纸
                BuildTask(120);
                rt = false;
            }
            else
            {
                //不存在文件,代表有新的壁纸
                BuildTask(1200);
                rt = true;
            }
            return rt;
        }

        private void BuildTask(uint minutes)
        {
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
            builder.SetTrigger(new MaintenanceTrigger(minutes, true));
            BackgroundTaskRegistration task = builder.Register();
        }

        private void OnVoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (this.deferral != null)
            {
                this.deferral.Complete();
            }
        }

        private void OntaskCancled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            System.Diagnostics.Debug.WriteLine("Task cancelled, clean up");
            if (this.deferral != null)
            {
                this.deferral.Complete();
            }
        }
    }
}
