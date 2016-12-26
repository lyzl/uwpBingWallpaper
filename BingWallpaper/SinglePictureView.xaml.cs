using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System.UserProfile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using BingWallpaper.Model;
using Microsoft.Toolkit.Uwp.UI.Animations;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BingWallpaper
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SinglePictureView : Page
    {
        public Wallpapers wallpapers;
        public Wallpaper appBackgroundImage { get; set; }

        public SinglePictureView()
        {
            this.InitializeComponent();
            getTodayPicture();
        }
        async void getTodayPicture()
        {
            wallpapers = await WallpaperProxy.GetWallpaper(1, 1);
            appBackgroundImage = wallpapers.images[0];
            Bindings.Update();
            //await AppBackgroundImage.Scale(1.1f, 1.1f, (float)AppBackgroundImage.ActualHeight / 2.0f, (float)AppBackgroundImage.ActualWidth / 2.0f, 1000, 0).StartAsync();
            //await AppBackgroundImage.Blur(100, 1000, 0).StartAsync();
            //await AppBackgroundImage.Offset(10, 10, 1000, 0).StartAsync();

        }

        private async void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            await WallpaperProxy.SaveAsWallpaper(wallpapers.images);
        }

        private async void SetAsLockscreenButton_Click(object sender, RoutedEventArgs e)
        {
            await UserProfilePersonalizationSettings.Current.TrySetLockScreenImageAsync(await StorageFile.GetFileFromPathAsync(wallpapers.images[0].url));
        }

        private void StarButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void SetAsDesktopButton_Click(object sender, RoutedEventArgs e)
        {
            await UserProfilePersonalizationSettings.Current.TrySetWallpaperImageAsync(await StorageFile.GetFileFromPathAsync(wallpapers.images[0].url));
        }
    }
}
