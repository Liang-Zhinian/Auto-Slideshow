﻿using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;

namespace AutoSlide.Models
{
    // A custom observable list to also contain the 
    public class MediaUrlList : ObservableCollection<MediaUrl>
    {
        private int index = 0;
        private int prefetched = 0;
        enum Direction { PREV, NEXT, NONE };
        Direction direction = Direction.NONE;
        bool _shuffle = false;
        ArrayList _proxyIndex = new ArrayList();

        public void addListener(PropertyChangedEventHandler del)
        {
            base.PropertyChanged += del;

        }

        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
                Changed();
            }
        }

        /*
                             // To prevent memory overflows, delete the prior images if too much memory is in use
                    //if (MemoryManager.AppMemoryUsage > 500000000 && base.Count > 5)
                    //{
                    //  MediaUrl prior = base[(Index > 1 ? Index - 2 : base.Count + Index - 2)];
                    //prior.clearMemoryAsync();
                    //}
                    if (base.Count > 3)
                    {
                        MediaUrl prev = base[(Index > 0 ? Index - 1 : base.Count - 1)];
                        MediaUrl next = base[(Index < base.Count - 1 ? Index + 1 : 0)];

                        if (!next.Image_retrieved && !next.failed)
                            Windows.System.Threading.ThreadPool.RunAsync((workitem) => { next.clearMemoryAsync(); }, Windows.System.Threading.WorkItemPriority.Low);
                        if (prev.Image_retrieved)
                            Windows.System.Threading.ThreadPool.RunAsync((workitem) => { prev.clearMemoryAsync(); }, Windows.System.Threading.WorkItemPriority.Low);
                    }

             */

        public void incrementPosition()
        {
            if (base.Count != 0)
            {

                int current = Index;
                int next = (current + 1) % base.Count;
                //if (_shuffle) {
                //    Random r = new Random();
                //    next = r.Next(base.Count);
                //}

                if (base.Count > 3)
                {
                    int nextplusone = (current + 1) % base.Count;

                    base[current].clearMemory();

                    switch (direction)
                    {
                        case Direction.PREV:
                            if (prefetched != -1)
                            {
                                base[prefetched].clearMemory();
                                prefetched = -1;
                            }
                            base[next].RetrieveContent();
                            break;
                        case Direction.NEXT:
                            break;
                        case Direction.NONE:
                            base[next].RetrieveContent();
                            break;

                    }

                    base[nextplusone].RetrieveContent();
                    prefetched = nextplusone;
                }
                direction = Direction.NEXT;
                Index = next;
            }
        }

        public void decrementPosition()
        {
            if (base.Count != 0)
            {

                int current = Index;

                int prev = current > 0 ? ((current - 1) % base.Count) : (base.Count - 1);

                //if (_shuffle)
                //{
                //    Random r = new Random();
                //    prev = r.Next(base.Count);
                //}

                if (base.Count > 3)
                {
                    int prevplusone = current > 1 ? ((current - 2) % base.Count) : (base.Count + current - 2);

                    base[current].clearMemory();


                    switch (direction)
                    {
                        case Direction.PREV:
                            break;
                        case Direction.NONE:
                            base[prev].clearMemory();
                            break;
                        case Direction.NEXT:
                            if (prefetched != -1)
                            {
                                base[prefetched].clearMemory();
                                prefetched = -1;
                            }
                            base[prev].RetrieveContent();
                            break;
                    }

                    base[prevplusone].RetrieveContent();
                    prefetched = prevplusone;
                }
                direction = Direction.PREV;
                Index = prev;
            }
        }

        public void setPosition(int position)
        {

            while (position < 0) position += base.Count;
            position = position % base.Count;
            while (position > base.Count) position -= base.Count;


            int current = Index;
            int prev = current > 0 ? (current - 1) % base.Count : base.Count - 1;
            int next = (current + 1) % base.Count;

            if (position == prev)
            {
                decrementPosition();
            }
            else if (position == next)
            {
                incrementPosition();
            }
            else if (position == current)
            {
                return;
            }
            else
            {

                base[current].clearMemory();
                base[position].RetrieveContent();
                Index = position;
                direction = Direction.NONE;
                if (prefetched != -1)
                {
                    base[prefetched].clearMemory();
                }
                prefetched = -1;
            }

        }

        public MediaUrl Url
        {
            private get
            {
                // To provide a smooth experience, we auto buffer the next and prior images
                if (base.Count != 0)
                {
                    MediaUrl current = base[Index];
                    if (!current.Image_retrieved && !current.Failed)
                    {
                        current.RetrieveContent();
                    }



                    return current;
                }
                else
                    throw new InvalidOperationException();

            }
            set
            {

                base[Index] = value;

                Changed();
            }
        }

        public async Task<MediaUrl> GetUrl()
        {
            // To provide a smooth experience, we auto buffer the next and prior images
            if (base.Count != 0)
            {
                int index = Index;
                if (_shuffle) index = (int)_proxyIndex[Index];
                MediaUrl current = base[index];
                if (!current.Image_retrieved && !current.Failed)
                {
                    await current.RetrieveContent();
                }



                return current;
            }
            else
                throw new InvalidOperationException();
        }

        public MediaUrlList() : base()
        {
        }

        public void Shuffle()
        {
            _shuffle = !_shuffle;
            if (_shuffle) {
                // randomize _proxyIndex

                Random r = new Random();

                for (int counter = 0; counter < this.Count; counter++)
                {
                    int newIndex = r.Next(this.Count);

                    // swap the two elements over 
                    int index = (int)_proxyIndex[counter];
                    _proxyIndex[counter] = _proxyIndex[newIndex];
                    _proxyIndex[newIndex] = index;
                }
            }
        }

        public new void Add(MediaUrl item)
        {
            item.PropertyChanged += (object sender, PropertyChangedEventArgs args) =>
            {
                base.OnPropertyChanged(args);
            };
            // Can not access the base element while not in the UI thread.
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                item.Index = base.Count;
                base.Add(item);
            });

            _proxyIndex.Add(base.Count);

        }

        public void changed()
        {
            Changed();
        }

        private void Changed([CallerMemberName] string propertyName = null)
        {

            base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        }
    }

    public class RandomMediaUrlList
    {
        MediaUrlList _list;
        MediaUrlList _originalList;
        bool _shuffle = false;
        public RandomMediaUrlList()
        {
            _list = new MediaUrlList();
        }

        public void Shuffle()
        {
            if (!_shuffle)
            {
                // backup and shuffle the _list
                _originalList = _list;

                Random r = new Random();
                MediaUrlList ultemp = new MediaUrlList();

                for (int counter = 0; counter < _list.Count; counter++)
                {
                    int newIndex = r.Next(_list.Count);

                    // swap the two elements over 
                    MediaUrl url = _list[counter];
                    _list[counter] = _list[newIndex];
                    _list[newIndex] = url;
                }

                _shuffle = true;
            }
            else {
                _list = _originalList;
            }
        }
    }
    // Class Representing a MediaItem - Also encapsulating the async call to retrieve the data
    public class MediaUrl : INotifyPropertyChanged
    {
        private Boolean image_retrieved;
        //public Task current_Task = null;

        public Boolean Image_retrieved
        {
            get
            {
                return image_retrieved;
            }
            set
            {
                image_retrieved = value;
                OnPropertyChanged();
            }
        }

        public Boolean failed;
        public Boolean Failed { get { return failed; } set { failed = value; OnPropertyChanged(); } }

        public int Index { get; set; }

        public Uri Image_Uri { get; set; }
        public Uri Image_Thumb_Uri { get; set; }

        public string Self { get; set; }
        public string Title { get; set; }

        public BitmapImage Image { get; set; }


        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public MediaUrl(String title, String imageUri, String imageThumbUri, string self_url)
        {
            Image_Uri = new Uri(imageUri);
            Image_Thumb_Uri = new Uri(imageThumbUri);
            image_retrieved = false;
            this.Title = title;
            Self = self_url;
            failed = false;
        }

        private async Task clearMemoryAsync()
        {
            if (!Image_retrieved || Failed) return;


            Image = null;

            image_retrieved = false;

        }
        public void clearMemory()
        {
            if (!Image_retrieved || Failed) return;


            Image = null;

            image_retrieved = false;
            /*
            if (current_Task == null || current_Task.IsCompleted || current_Task.IsCanceled || current_Task.IsFaulted)
            {
                // current task can be discarded
                current_Task = clearMemoryAsync();
            }
            else
            {
                current_Task = current_Task.ContinueWith(async (task) =>
                {
                    await clearMemoryAsync();
                });
            }*/
        }

        public async Task RetrieveContent()
        {
            if (Image_retrieved) return;
            //Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            if (!Image_Uri.IsFile)
            {


                //StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("PickedFolderToken")


                Image = new BitmapImage();
                Image.UriSource = Image_Uri;
                Image.ImageOpened += (object sender, Windows.UI.Xaml.RoutedEventArgs args) =>
                {
                    Image_retrieved = true;
                };
                Image.ImageFailed += (object sender, Windows.UI.Xaml.ExceptionRoutedEventArgs args) =>
                {
                    Image_retrieved = true;
                    failed = true;

                };

            }
            else
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(Image_Uri.LocalPath);
                IRandomAccessStream irandom = await file.OpenReadAsync();

                Image = new BitmapImage();
                Image.ImageOpened += (object sender, Windows.UI.Xaml.RoutedEventArgs args) =>
                {
                    Image_retrieved = true;
                };
                Image.ImageFailed += (object sender, Windows.UI.Xaml.ExceptionRoutedEventArgs args) =>
                {
                    Image_retrieved = true;
                    failed = true;

                };

                Image.SetSource(irandom);
            }
            /*
            if(current_Task == null || current_Task.IsCompleted || current_Task.IsCanceled || current_Task.IsFaulted)
            {
                // current task can be discarded
                current_Task = RetrieveContentAsync();
            } else
            {
                current_Task = current_Task.ContinueWith(async (task) =>
                {
                    await RetrieveContentAsync();
                });
            }*/
        }

        private async Task RetrieveContentAsync()
        {
            if (Image_retrieved) return;
            //Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {



                Image = new BitmapImage(Image_Uri);
                Image.ImageOpened += (object sender, Windows.UI.Xaml.RoutedEventArgs args) =>
                {
                    Image_retrieved = true;
                };
                Image.ImageFailed += (object sender, Windows.UI.Xaml.ExceptionRoutedEventArgs args) =>
                {
                    Image_retrieved = true;
                    failed = true;

                };

            }
            //);

            /*// If we're already retrieving the image, wait.
            await retrievingContent.WaitAsync();
           
            try {
                if (Image_retrieved || failed) return;

                try { 
                await totalRequestSemaphore.WaitAsync();
                // For http requests, as often reddit img urls redirect https -> http, which is not supported by default
                // we have to manually perform redirects.
                HttpClientHandler handler = new HttpClientHandler();
                handler.AllowAutoRedirect = false;
                HttpClient client = new HttpClient(handler);
                
                HttpResponseMessage response = await client.GetAsync(Image_Uri);

                while(response.StatusCode == System.Net.HttpStatusCode.Redirect || response.StatusCode == System.Net.HttpStatusCode.MovedPermanently)
                {
                    // Redirect required.
                    Uri redirect_uri = response.Headers.Location;
                    response = await client.GetAsync(redirect_uri);
                }
                
                    byte[] img = await response.Content.ReadAsByteArrayAsync();
                    InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
                    using (DataWriter writer = new DataWriter(randomAccessStream.GetOutputStreamAt(0))) {
                        writer.WriteBytes(img);
                        await writer.StoreAsync();
                    }

                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Image = new BitmapImage();
                        Image.SetSource(randomAccessStream);

                      

                        randomAccessStream.Dispose();
                        Image_retrieved = true;
                    });
                
            } catch(System.Threading.Tasks.TaskCanceledException e)
            {
                Debug.WriteLine(e);
                failed = true;
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine(e);
                failed = true;
            }
                finally
                {
                    totalRequestSemaphore.Release();

                }
            }
            finally
            {

                retrievingContent.Release();
            }
        */
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {

            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}