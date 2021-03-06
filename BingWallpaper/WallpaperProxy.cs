﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using BingWallpaper.Model;

namespace BingWallpaper
{
    public class WallpaperProxy
    {
        
        public static async Task<Wallpapers> GetWallpaper(int index, int n)
        {
            bool internetConnected = false;
            var internetConnectionProfile = NetworkInformation.GetConnectionProfiles();
            if (internetConnectionProfile.Count != 0)
            {
                foreach (var item in internetConnectionProfile)
                {
                    if (item.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess)
                    {
                        internetConnected = true;
                    }
                }
            }
            if (!internetConnected)
            {
                await new MessageDialog("获取失败，请检查网络连接").ShowAsync();
                return null;
            }

            var http = new HttpClient();
            var url = String.Format("http://www.bing.com/HPImageArchive.aspx?format=js&idx={0}&n={1}&mkt=en-US", index, n);
            var response = await http.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            var serializer = new DataContractJsonSerializer(typeof(Wallpapers));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
            var data = (Wallpapers)serializer.ReadObject(ms);
            foreach (var item in data.images)
            {
                if (!item.url.StartsWith(@"http://s.cn.bing.net"))
                {
                    item.url = @"http://www.bing.com/" + item.url;
                }
            }
            await SaveWallPaper(data.images);
            return data;
        }
        public static async Task<List<IStorageFile>> SaveWallPaper(List<Wallpaper> Wallpapers)
        {
            List<IStorageFile> fileList = new List<IStorageFile>();
            foreach (var item in Wallpapers)
            {
                var path = string.Format("{0}\\{1}\\{2}.jpg", ApplicationData.Current.LocalFolder.Path, "Wallpapers", item.fullstartdate);
                if (File.Exists(path))
                {
                    item.url = path;
                    continue;
                }
                List<Byte> allBytes = new List<byte>();
                // 把壁纸的图片文件保存到当前的应用文件夹里面
                using (var response = await HttpWebRequest.Create(item.url).GetResponseAsync())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {

                        byte[] buffer = new byte[4000];
                        int bytesRead = 0;
                        while ((bytesRead = await responseStream.ReadAsync(buffer, 0, 4000)) > 0)
                        {
                            allBytes.AddRange(buffer.Take(bytesRead));
                        }
                    }
                }

                IStorageFolder WallpapersFolder;
                Debug.WriteLine(ApplicationData.Current.LocalFolder.Path);
                if (!Directory.Exists(string.Format("{0}\\{1}", ApplicationData.Current.LocalFolder.Path, "Wallpapers")))
                {
                    WallpapersFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Wallpapers");
                }
                else
                {
                    WallpapersFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Wallpapers");
                }
                IStorageFile saveFile = await WallpapersFolder.CreateFileAsync(item.fullstartdate + ".jpg", CreationCollisionOption.ReplaceExisting);
                item.url = saveFile.Path;
                await FileIO.WriteBytesAsync(saveFile, allBytes.ToArray());
                fileList.Add(saveFile);
            }
            await SaveWallpaperInfo(Wallpapers);
            return fileList;
        }
        public static async Task<List<IStorageFile>> SaveWallpaperInfo(List<Wallpaper> Wallpapers)
        {
            List<IStorageFile> fileList = new List<IStorageFile>();
            IStorageFolder WallpapersInfoFolder;
            if (!Directory.Exists(string.Format("{0}\\{1}", ApplicationData.Current.LocalFolder.Path, "WallpapersInfo")))
            {
                WallpapersInfoFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("WallpapersInfo");
            }
            else
            {
                WallpapersInfoFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("WallpapersInfo");
            }
            foreach (var item in Wallpapers)
            {
                if (File.Exists(item.fullstartdate + ".txt"))
                {
                    continue;
                }
                IStorageFile infoFile = await WallpapersInfoFolder.CreateFileAsync(item.fullstartdate + ".txt", CreationCollisionOption.ReplaceExisting);
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Wallpaper));
                using (Stream file = await infoFile.OpenStreamForWriteAsync())
                {
                    serializer.WriteObject(file, item);
                }
                fileList.Add(infoFile);
            }
            return fileList;
        }
        public static async Task<List<Wallpaper>> GetWallpaperInfoFromFile()
        {
            IStorageFolder fileFolder;
            List<Wallpaper> wallpaperList = new List<Wallpaper>();
            if (!Directory.Exists(string.Format("{0}\\{1}", ApplicationData.Current.LocalFolder.Path, "WallpapersInfo")))
            {
                return null;
            }
            else
            {
                fileFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("WallpapersInfo");
            }
            var fileList = await fileFolder.GetFilesAsync();
            var num = fileList.Count;
            var serializer = new DataContractJsonSerializer(typeof(Wallpaper));
            foreach (var item in fileList)
            {
                var stream = await item.OpenAsync(FileAccessMode.Read);

                var data = (Wallpaper)serializer.ReadObject(stream.AsStreamForRead());
                wallpaperList.Add(data);
            }
            return wallpaperList;
        }

        public static async Task<List<StorageFile>> SaveAsWallpaper(List<Wallpaper> Wallpapers)
        {
            var rt = new List<StorageFile>();
            if (Wallpapers.Count == 1)
            {
                var filePicker = new FileSavePicker();
                filePicker.FileTypeChoices.Add("JPEG image file", new List<string> { ".jpg" });
                filePicker.SuggestedFileName = Wallpapers[0].fullstartdate;
                StorageFile file1 = await StorageFile.GetFileFromPathAsync(Wallpapers[0].url);
                var file2 = await filePicker.PickSaveFileAsync();
                if (file2 != null)
                {
                    await file1.CopyAndReplaceAsync(file2);
                    rt.Add(file2);
                }
            }
            else if (Wallpapers.Count > 1)
            {
                var folderPicker = new FolderPicker();
                var destFolder = await folderPicker.PickSingleFolderAsync();
                if (destFolder != null)
                {
                    foreach (var item in Wallpapers)
                    {
                        var file1 = await StorageFile.GetFileFromPathAsync(item.url);
                        var file2 = await file1.CopyAsync(destFolder);
                        rt.Add(file2);
                    }
                }
            }
            return rt;
        }

        public static async Task refreshCurrentWallpaper()
        {
            Wallpapers Wallpapers = await GetWallpaper(1, 14);
            await SaveWallpaperInfo(Wallpapers.images);
        }

    }
}
