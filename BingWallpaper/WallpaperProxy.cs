using System;
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
using Windows.Storage;
using Windows.Storage.Pickers;

namespace BingWallpaper
{
    public class WallpaperProxy
    {

        public static async Task<WallPapers> GetWallpaper(int index, int n)
        {
            var http = new HttpClient();
            var url = String.Format("http://www.bing.com/HPImageArchive.aspx?format=js&idx={0}&n={1}&mkt=en-US", index, n);
            var response = await http.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            var serializer = new DataContractJsonSerializer(typeof(WallPapers));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
            var data = (WallPapers)serializer.ReadObject(ms);

            foreach (var item in data.images)
            {
                var path = string.Format("{0}\\{1}\\{2}.jpg", ApplicationData.Current.LocalFolder.Path, "wallpapers", item.fullstartdate);
                if (File.Exists(path))
                {
                    item.url = path;
                }
            }
            return data;
        }
        public static async Task<List<IStorageFile>> SaveWallPaper(List<Wallpaper> wallpapers)
        {
            List<IStorageFile> fileList = new List<IStorageFile>();
            foreach (var item in wallpapers)
            {
                List<Byte> allBytes = new List<byte>();
                // 把壁纸的图片文件保存到当前的应用文件里面
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

                IStorageFolder wallpapersFolder;
                Debug.WriteLine(ApplicationData.Current.LocalFolder.Path);
                if (!Directory.Exists(string.Format("{0}\\{1}", ApplicationData.Current.LocalFolder.Path, "wallpapers")))
                {
                    wallpapersFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("wallpapers");
                }
                else
                {
                    wallpapersFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("wallpapers");
                }
                IStorageFile saveFile = await wallpapersFolder.CreateFileAsync(item.fullstartdate + ".jpg", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBytesAsync(saveFile, allBytes.ToArray());
                fileList.Add(saveFile);
            }
            await SaveWallpaperInfo(wallpapers);
            return fileList;
        }
        public static async Task<List<IStorageFile>> SaveWallpaperInfo(List<Wallpaper> wallpapers)
        {
            List<IStorageFile> fileList = new List<IStorageFile>();
            IStorageFolder wallpapersInfoFolder;
            if (!Directory.Exists(string.Format("{0}\\{1}", ApplicationData.Current.LocalFolder.Path, "wallpapersInfo")))
            {
                wallpapersInfoFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("wallpapersInfo");
            }
            else
            {
                wallpapersInfoFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("wallpapersInfo");
            }
            foreach (var item in wallpapers)
            {
                if (File.Exists(item.fullstartdate + ".txt"))
                {
                    continue;
                }
                IStorageFile infoFile = await wallpapersInfoFolder.CreateFileAsync(item.fullstartdate + ".txt", CreationCollisionOption.ReplaceExisting);
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
            if (!Directory.Exists(string.Format("{0}\\{1}", ApplicationData.Current.LocalFolder.Path, "wallpapersInfo")))
            {
                return null;
            }
            else
            {
                fileFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("wallpapersInfo");
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

        public static async Task<List<StorageFile>> SaveAsWallpaper(List<Wallpaper> wallpapers)
        {
            var rt = new List<StorageFile>();
            if (wallpapers.Count == 1)
            {
                var filePicker = new FileSavePicker();
                filePicker.FileTypeChoices.Add("JPEG image file", new List<string> { ".jpg" });
                filePicker.SuggestedFileName = wallpapers[0].fullstartdate;
                StorageFile file1 = await StorageFile.GetFileFromPathAsync(wallpapers[0].url);
                var file2 = await filePicker.PickSaveFileAsync();
                if (file2 != null)
                {
                    await file1.CopyAndReplaceAsync(file2);
                    rt.Add(file2);
                }
            }
            else if (wallpapers.Count > 1)
            {
                var folderPicker = new FolderPicker();
                var destFolder = await folderPicker.PickSingleFolderAsync();
                if (destFolder != null)
                {
                    foreach (var item in wallpapers)
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
            WallPapers wallPapers = await GetWallpaper(1, 14);
            await SaveWallpaperInfo(wallPapers.images);
        }

    }

    [DataContract]
    public class Wallpaper
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
        public List<object> hs { get; set; }
    }

    [DataContract]
    public class Tooltips
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
    public class WallPapers
    {
        [DataMember]
        public List<Wallpaper> images { get; set; }
        [DataMember]
        public Tooltips tooltips { get; set; }
    }

}
