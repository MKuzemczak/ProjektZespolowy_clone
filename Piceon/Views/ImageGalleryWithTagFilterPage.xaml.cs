using System;
using System.Collections.Generic;
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

using Piceon.Models;
using System.Threading.Tasks;

namespace Piceon.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImageGalleryWithTagFilterPage : Page
    {
        public event EventHandler ImageClicked;

        public ImageGalleryWithTagFilterPage()
        {
            this.InitializeComponent();
        }

        private void ImageGalleryPage_ImageClicked(object sender, EventArgs e)
        {
            ImageClicked?.Invoke(this, e);
        }
        public async Task AccessFolder(FolderItem folder)
        {
            await imageGalleryPage.AccessFolder(folder);
            tagFilterPage.SetTagList(await folder.GetTagsOfImagesAsync());
        }

        private async void TagFilterPage_SelectedTagsChanged(object sender, SelectedTagsChangedEventArgs e)
        {
            await imageGalleryPage.SetTagsToFilter(e.SelectedTags);
        }
    }
}
