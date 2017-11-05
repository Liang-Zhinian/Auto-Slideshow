using AutoSlide.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml.Controls;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AutoSlide.Views
{


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Slideshow : Page
    {
        private async Task<List<MediaUrl>> GenerateLocalImages(List<string> urls, CancellationToken token)
        {
            // Regex for checking image items
            Regex image_expr = new Regex(@".(?:jpg|jpeg|png|bmp|gif|gifv)$");
            Regex gifv_expr = new Regex(@".(?:gifv)$");

            // Regex for identifying non-type-labelled imgur links
            Regex imgur_album = new Regex(@"(?:[a-zA-Z0-0]\.)?imgur.com\/(?:a|gallery|t\/[a-zA-Z0-9]*)\/([a-zA-Z0-9]*)");
            Regex imgur_unmarked = new Regex(@"(?:[a-zA-Z0-0]\.)?imgur.com\/([a-zA-Z0-9]{3,})");

            // Regex for identifying gfycat(beautifully simple) links
            Regex gfycat_links = new Regex(@"gfycat.com\/([a-zA-Z0-9]*)");
            int imgur_remaining = 0;
            int total_link_count = urls.Count;
            int current_link_count = 0;
            int current_image_count = 0;

            List<MediaUrl> list = new List<MediaUrl>();

            Stopwatch watch = new Stopwatch();
            watch.Start();

            foreach (string path in urls)
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
                IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                
                foreach (StorageFile file in files)
                {
                    if (file != null)
                    {
                        string title = file.Name;
                        string url = file.Path;
                        string thumb_url = url;
                        string self_url = url;

                        MediaUrl media_obj = new MediaUrl(title, url, thumb_url, self_url);
                        //await media_obj.RetrieveContent();
                        list.Add(media_obj);
                        current_image_count++;

                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            ImagesCountText.Text = "Loaded " + current_image_count + " Images";
                        });
                    }
                }

                current_link_count++;
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    OverallProgressText.Text = "Processed " + current_link_count + " / " + total_link_count + " Links";
                    OverallProgressLoader.Value = (float)current_link_count / total_link_count * 100;
                });
            }

            return list;
        }
    }

}
