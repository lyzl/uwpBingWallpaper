using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using BingWallpaper.Model;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace BingWallpaper
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private List<NavBarMenuItem> sideBarMenuItems;
        public MainPage()
        {
            this.InitializeComponent();
            this.PicturesPage.Navigate(typeof(SinglePictureView));
            sideBarMenuItems = NavBarMenuItem.GetNavBarMenuItems();
            Debug.WriteLine(Windows.Storage.ApplicationData.Current.LocalFolder.Path);
        }

        //private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        //{
        //    NavBar.IsPaneOpen = !NavBar.IsPaneOpen;
        //    if (NavBar.IsPaneOpen)
        //    {
        //        VisualStateManager.GoToState(this, "PaneClose", false);
        //    }
        //}

        private void NavBarItems_ItemClick(object sender, ItemClickEventArgs e)
        {
            var menuItem = (NavBarMenuItem)e.ClickedItem;
            PicturesPage.Navigate(menuItem.destPage);
            NavBar.IsPaneOpen = false;
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            PicturesPage.Navigate(typeof(SettingPage));
            NavBar.IsPaneOpen = false;
        }

        private void HamburgerButton_Checked(object sender, RoutedEventArgs e)
        {
            NavBar.IsPaneOpen = true;
            VisualStateManager.GoToState(this, "PaneOpen", false);
        }

        private void HamburgerButton_Unchecked(object sender, RoutedEventArgs e)
        {
            NavBar.IsPaneOpen = false;
            VisualStateManager.GoToState(this, "PaneClose", false);
        }
    }
}
