using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace VoiceCommandService
{
    public sealed class WallpaperProxy
    {
        public static IAsyncOperation<Wallpapers> GetWallpaper(int index, int n)
        {
            return GetWallpaperHelper(index, n).AsAsyncOperation();
        }
        private static async Task<Wallpapers> GetWallpaperHelper(int index, int n)
        {
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
            return data;
        }
        public static IAsyncOperation<IStorageFile> SaveWallPaper(Wallpaper wallpaper)
        {
            return SaveWallPaperHelper(wallpaper).AsAsyncOperation();
        }
        private static async Task<IStorageFile> SaveWallPaperHelper(Wallpaper wallpaper)
        {
            List<Byte> allBytes = new List<byte>();
            // 把壁纸的图片文件保存到当前的应用文件里面
            using (var response = await HttpWebRequest.Create(wallpaper.url).GetResponseAsync())
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
            IStorageFile saveFile = await WallpapersFolder.CreateFileAsync(wallpaper.fullstartdate + ".jpg", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(saveFile, allBytes.ToArray());
            await WallpaperProxy.SaveWallpaperInfo(wallpaper);
            return saveFile;
        }

        public static IAsyncOperation<IStorageFile> SaveWallpaperInfo(Wallpaper wallpaper)
        {
            return SaveWallpaperInfoHelper(wallpaper).AsAsyncOperation();
        }
        private static async Task<IStorageFile> SaveWallpaperInfoHelper(Wallpaper wallpaper)
        {
            IStorageFolder WallpapersInfoFolder;
            if (!Directory.Exists(string.Format("{0}\\{1}", ApplicationData.Current.LocalFolder.Path, "WallpapersInfo")))
            {
                WallpapersInfoFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("WallpapersInfo");
            }
            else
            {
                WallpapersInfoFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("WallpapersInfo");
            }
            IStorageFile infoFile = await WallpapersInfoFolder.CreateFileAsync(wallpaper.fullstartdate + ".txt", CreationCollisionOption.ReplaceExisting);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Wallpaper));
            using (Stream file = await infoFile.OpenStreamForWriteAsync())
            {
                using (StreamWriter writer = new StreamWriter(file))
                {
                    serializer.WriteObject(file, wallpaper);
                }
            }
            return infoFile;
        }

    }
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
        public IList<object> hs { get; set; }
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
        public IList<Wallpaper> images { get; set; }
        [DataMember]
        public Tooltips tooltips { get; set; }
    }

}
