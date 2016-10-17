using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using BingWallpaper.Model;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BingWallpaper
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MultiPicturesView : Page
    {
        public ObservableCollection<Wallpaper> WallpapersCollection { get; set; }
        //double thumbnailHeight;
        //double thumbnailWidth;
        public MultiPicturesView()
        {
            //thumbnailHeight = (double)ApplicationData.Current.LocalSettings.Values["screenHeight"] / 6.0;
            //thumbnailWidth = (double)ApplicationData.Current.LocalSettings.Values["screenWidth"] / 6.0;
            this.InitializeComponent();
            WallpapersCollection = new ObservableCollection<Wallpaper>();
            getPastPicture();
            VisualStateManager.GoToState(this, "OverviewState", false);
        }
        public async void getPastPicture()
        {
            await WallpaperProxy.refreshCurrentWallpaper();
            List<Wallpaper> wallpaperList;
            wallpaperList = await WallpaperProxy.GetWallpaperInfoFromFile();

            foreach (var item in wallpaperList)
            {
                WallpapersCollection.Add(item);
            }
        }

        private void MainGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            VisualStateManager.GoToState(this, "DetailState", false);
        }

        private void MainImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "OverviewState", false);
        }
    }
}
