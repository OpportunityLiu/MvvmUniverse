using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using ShareEventHandler = Windows.Foundation.TypedEventHandler<Windows.ApplicationModel.DataTransfer.DataTransferManager, Windows.ApplicationModel.DataTransfer.DataRequestedEventArgs>;

namespace Opportunity.MvvmUniverse.Commands.Predefined
{
    /// <summary>
    /// Commands for Windows.ApplicationModel.DataTransfer.
    /// </summary>
    public static class DataTransferCommands
    {
        private static DataPackage pack(object data)
        {
            if (!(data is DataPackage dp))
            {
                dp = new DataPackage
                {
                    Properties =
                    {
                        Title = Package.Current.DisplayName,
                        ApplicationName = Package.Current.DisplayName,
                        PackageFamilyName = Package.Current.Id.FamilyName,
                    },
                    RequestedOperation = DataPackageOperation.Copy,
                };
                switch (data)
                {
                case string s:
                    dp.SetText(s);
                    break;
                case IStorageItem si:
                    var files = new[] { si };
                    PrepareFileShare(dp, files);
                    dp.SetStorageItems(new[] { si });
                    break;
                case IEnumerable<IStorageItem> sis:
                    PrepareFileShare(dp, sis);
                    dp.SetStorageItems(sis);
                    break;
                case RandomAccessStreamReference bitmap:
                    dp.SetBitmap(bitmap);
                    break;
                case Uri uri:
                    dp.SetWebLink(uri);
                    dp.SetText(uri.ToString());
                    break;
                default:
                    dp.SetText(data.ToString());
                    break;
                }
            }
            return dp;

            void PrepareFileShare(DataPackage package, IEnumerable<IStorageItem> files)
            {
                package.RequestedOperation = DataPackageOperation.Move;
                foreach (var item in files.OfType<IStorageFile>().Select(f => f.FileType).Distinct())
                {
                    if (item != null)
                        package.Properties.FileTypes.Add(item);
                }
            }
        }

        /// <summary>
        /// Set data to clipboard,
        /// accepted type: <see cref="DataPackage"/>, <see cref="string"/>, <see cref="System.Uri"/>, <see cref="IStorageItem"/>, <see cref="IEnumerable"/>&lt;<see cref="IStorageItem"/>&gt; and <see cref="RandomAccessStreamReference"/> of bitmap,
        /// other items will be converted to <see cref="string"/>.
        /// </summary>
        public static Command<object> SetClipboard { get; } = Command.Create<object>((c, o) =>
        {
            var dp = pack(o);
            Clipboard.SetContent(dp);
            Clipboard.Flush();
        }, (c, o) => o != null);

        /// <summary>
        /// Clear data of clipboard.
        /// </summary>
        public static Command ClearClipboard { get; } = Command.Create((c) =>
        {
            Clipboard.Clear();
            Clipboard.Flush();
        });


        /// <summary>
        /// Use share UI to share contents,
        /// accepted type: <see cref="DataPackage"/>, <see cref="string"/>, <see cref="System.Uri"/>, <see cref="IStorageItem"/>, <see cref="IEnumerable"/>&lt;<see cref="IStorageItem"/>&gt; and <see cref="RandomAccessStreamReference"/> of bitmap,
        /// other items will be converted to <see cref="string"/>.
        /// </summary>
        public static AsyncCommand<object> Share { get; } = AsyncCommand.Create<object>((c, o) =>
        {
            return DataTransferManager.GetForCurrentView().ShareAsync(pack(o));
        }, (c, o) => DataTransferManager.IsSupported() ? o != null : false);
        /// <summary>
        /// Use share UI to share contents.
        /// </summary>
        public static AsyncCommand<Func<DataPackage>> ShareWithProvider { get; }
            = AsyncCommand.Create<Func<DataPackage>>((c, o) =>
        {
            return DataTransferManager.GetForCurrentView().ShareAsync(o);
        }, (c, o) => DataTransferManager.IsSupported() ? o != null : false);
        /// <summary>
        /// Use share UI to share contents.
        /// </summary>
        public static AsyncCommand<Func<IAsyncOperation<DataPackage>>> ShareWithAsyncProvider { get; }
            = AsyncCommand.Create<Func<IAsyncOperation<DataPackage>>>((c, o) =>
        {
            return DataTransferManager.GetForCurrentView().ShareAsync(o);
        }, (c, o) => DataTransferManager.IsSupported() ? o != null : false);
    }
}
