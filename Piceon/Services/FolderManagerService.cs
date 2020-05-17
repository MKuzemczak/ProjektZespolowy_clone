using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Piceon.DatabaseAccess;
using Piceon.Models;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Search;

namespace Piceon.Services
{
    public static class FolderManagerService
    {
        private static FolderItem CurrentlyScannedFolder { get; set; }
        private static StateMessage RecentStateMessage { get; set; }

        private static StateMessagingService StateMessaging { get; } = StateMessagingService.Instance;

        private static Dictionary<int, List<ImageItem>> FolderPickImages = new Dictionary<int, List<ImageItem>>();
        private static int FolderPickCntr = 0;

        private static Dictionary<int, List<int>> FolderPickTasks = new Dictionary<int, List<int>>();
        private static Dictionary<int, FolderItem> FolderPickFolder = new Dictionary<int, FolderItem>();
        private static Dictionary<int, StateMessage> FolderPickStateMessage = new Dictionary<int, StateMessage>();

        public static async Task<FolderItem> OpenFolderAsync()
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };

            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder is object)
            {
                var dbvf = await AddToDatabase(folder);
                string token = StorageApplicationPermissions.FutureAccessList.Add(folder);
                return await FolderItem.FromDatabaseVirtualFolder(dbvf);
            }

            return null;
        }

        private static async Task<DatabaseVirtualFolder> AddToDatabase(StorageFolder folder, int parentId = -1)
        {
            var dbvf = await DatabaseAccessService.InsertVirtualFolderAsync(folder.Name, parentId);
            var subs = await folder.GetFoldersAsync();

            foreach(var item in subs)
            {
                await AddToDatabase(item, dbvf.Id);
            }

            List<string> fileTypeFilter = new List<string>();
            fileTypeFilter.Add(".jpg");
            fileTypeFilter.Add(".png");
            fileTypeFilter.Add(".bmp");
            fileTypeFilter.Add(".gif");
            var queryOptions = new QueryOptions(CommonFileQuery.OrderByName, fileTypeFilter);
            queryOptions.FolderDepth = FolderDepth.Shallow;
            var files = await folder.CreateFileQueryWithOptions(queryOptions).GetFilesAsync();

            foreach (var file in files)
            {
                await DatabaseAccessService.InsertImageAsync(file.Path, false, dbvf.Id);
            }

            return dbvf;
        }

        public static async Task<List<FolderItem>> GetAllFolders()
        {
            var result = new List<FolderItem>();

            var virtualFoldersRootNodes = await DatabaseAccessService.GetRootVirtualFoldersAsync();

            foreach (var item in virtualFoldersRootNodes)
            {
                result.Add(await FolderItem.FromDatabaseVirtualFolder(item));
            }

            return result;
        }

        public static async Task PickAndImportImagesToFolder(FolderItem folder)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");

            var files = await picker.PickMultipleFilesAsync();
            if (files != null && files.Count > 0)
            {
                var recentStateMessage = StateMessaging.SendLoadingMessage("Scanning imported images...");
                var imageItems = await folder.AddFilesToFolder(files);

                CurrentlyScannedFolder = folder;
                try
                {
                    await BackendConctroller.TagImages(imageItems.Select(i => i.DatabaseId).ToList());
                }
                catch (Exception)
                {

                }

                var pickTaskIds = new List<int>();
                int compareTaskId = BackendConctroller.CompareImages(
                    imageItems,
                    FindSimilarFinishedHandler);

                pickTaskIds.Add(compareTaskId);
                FolderPickImages.Add(FolderPickCntr, imageItems);
                FolderPickTasks.Add(FolderPickCntr, pickTaskIds);
                FolderPickFolder.Add(FolderPickCntr, folder);
                FolderPickStateMessage.Add(FolderPickCntr, recentStateMessage);
            }
            FolderPickCntr++;
        }

        private static async void FindSimilarFinishedHandler(ControllerTaskResultMessage result)
        {
            if (result.result != BackendConctroller.DoneMessage)
                return;

            if (!FolderPickTasks.Any(i => i.Value.Contains(result.taskid)))
                return;

            if (result.images.Count == 0)
                return;

            int folderPick = -1;
            foreach (var item in FolderPickTasks)
            {
                if (item.Value.Remove(result.taskid))
                {
                    folderPick = item.Key;
                    break;
                }
            }

            foreach (var group in result.images)
            {
                await FolderPickFolder[folderPick].GroupImages(group);
            }

            await CheckAllTasksInFolderPickDone(folderPick);
            await CurrentlyScannedFolder.UpdateQueryAsync();
        }

        private static async Task CheckAllTasksInFolderPickDone(int folderPick)
        {
            if (FolderPickTasks[folderPick].Count > 0)
                return;

            await MarkImagesScanned(FolderPickImages[folderPick]);
            FolderPickTasks.Remove(folderPick);
            FolderPickImages.Remove(folderPick);
            FolderPickFolder.Remove(folderPick);
            StateMessaging.RemoveMessage(FolderPickStateMessage[folderPick]);
            FolderPickStateMessage.Remove(folderPick);
            StateMessaging.SendInfoMessage("Images scanned successfully", 5000);
        }

        private static async Task MarkImagesScanned(List<ImageItem> imageItems)
        {
            foreach (var item in imageItems)
            {
                await item.MarkScannedAsync();
            }
        }
    }
}
