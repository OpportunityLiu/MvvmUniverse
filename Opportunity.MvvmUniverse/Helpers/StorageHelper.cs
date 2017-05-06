using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Windows.Foundation;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace Windows.Storage
{
    /// <summary>
    /// 用于操作缓存文件夹的辅助类
    /// </summary>
    public static class StorageHelper
    {
        private readonly static StorageFolder temp = ApplicationData.Current.TemporaryFolder;

        public static IAsyncOperation<StorageItemThumbnail> GetIconOfExtension(string extension)
        {
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));
            var filename = default(string);
            if (string.IsNullOrWhiteSpace(extension))
                filename = $"Dummy{"".GetHashCode()}";
            else
            {
                var ext = extension.Trim().TrimStart('.');
                filename = $"Dummy{ext.GetHashCode()}.{ext}";
            }
            return Run(async token =>
            {
                var dummy = await temp.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
                return await dummy.GetThumbnailAsync(ThumbnailMode.SingleItem);
            });
        }

        public static IAsyncOperation<StorageFile> TryGetFileAsync(this StorageFolder folder, string name)
        {
            return Run(async token => await folder.TryGetItemAsync(name) as StorageFile);
        }
        public static IAsyncOperation<StorageFolder> TryGetFolderAsync(this StorageFolder folder, string name)
        {
            return Run(async token => await folder.TryGetItemAsync(name) as StorageFolder);
        }

        public static IAsyncOperation<StorageFile> SaveFileAsync(this StorageFolder folder, string fileName, CreationCollisionOption options, IBuffer buffer)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));
            return Run(async token =>
            {
                fileName = ToValidFileName(fileName);
                var file = await folder.CreateFileAsync(fileName, options);
                await FileIO.WriteBufferAsync(file, buffer);
                return file;
            });
        }

        public static IAsyncOperation<StorageFolder> CreateTempFolderAsync()
        {
            return temp.CreateFolderAsync(ToValidFileName(null));
        }

        private static Dictionary<char, char> alternateFolderChars = new Dictionary<char, char>()
        {
            ['?'] = '？',
            ['\\'] = '＼',
            ['/'] = '／',
            ['"'] = '＂',
            ['|'] = '｜',
            ['*'] = '＊',
            ['<'] = '＜',
            ['>'] = '＞',
            [':'] = '：'
        };

        private static char[] invalidChars = Path.GetInvalidFileNameChars();

        public static string ToValidFileName(string raw)
        {
            if (raw == null)
                return DateTimeOffset.Now.Ticks.ToString();
            if (raw.IndexOfAny(invalidChars) == -1)
            {
                if (string.IsNullOrWhiteSpace(raw))
                    return DateTimeOffset.Now.Ticks.ToString();
                else
                    return raw;
            }
            var sb = new StringBuilder(raw);
            foreach (var item in alternateFolderChars)
            {
                sb.Replace(item.Key, item.Value);
            }
            foreach (var item in invalidChars)
            {
                sb.Replace(item.ToString(), "");
            }
            var final = sb.ToString().Trim();
            if (string.IsNullOrEmpty(final))
                return DateTimeOffset.Now.Ticks.ToString();
            return final;
        }
    }
}
