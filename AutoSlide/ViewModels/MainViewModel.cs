using AutoSlide.Models;
using AutoSlide.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace AutoSlide.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // Variables
        private bool _isPaneOpen;
        private ObservableCollection<MenuItem> _menuItems;
        private MenuItem _selectedMenuItem;

        private Frame appFrame;
        private Frame splitViewFrame;
        private bool isBusy;



        public Frame AppFrame
        {
            get { return appFrame; }
        }
        public Frame SplitViewFrame
        {
            get { return splitViewFrame; }
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                NotifyPropertyChanged("IsBusy");
            }
        }


        public ObservableCollection<MenuItem> MenuItems
        {
            get { return _menuItems; }
            set
            {
                _menuItems = value;
                NotifyPropertyChanged("MenuItems");
            }
        }

        public MenuItem SelectedMenuItem
        {
            get { return _selectedMenuItem; }
            set
            {
                _selectedMenuItem = value;
                NotifyPropertyChanged("SelectedMenuItem");

                Navigate(_selectedMenuItem.View);
            }
        }


        public MainViewModel()
        {
            
        }

        
        public bool IsDataLoaded
        {
            get;
            private set;
        }

        public bool IsPaneOpen
        {
            get
            {
                return _isPaneOpen;
            }
            set
            {
                _isPaneOpen = value;
                NotifyPropertyChanged("IsPaneOpen");
            }
        }




        private void Navigate(Type view)
        {
            var type = view.Name;

            switch (type)
            {
                case "LocalMainPage":
                    SplitViewFrame.Navigate(view);
                    break;
                case "RedditMainPage":
                    SplitViewFrame.Navigate(view);
                    break;
                case "AboutView":
                    SplitViewFrame.Navigate(view);
                    break;
            }
        }
        



        private string _rawContent;
        public string RawContent
        {
            get
            {
                if (_rawContent == null) _rawContent = "";

                return _rawContent;
            }
            set
            {
                if (value != _rawContent)
                {
                    _rawContent = value;
                    NotifyPropertyChanged("RawContent");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void InitialMenuItems()
        {
            MenuItems = new ObservableCollection<MenuItem>
            {
                new MenuItem
                {
                    //Icon = "",
                    Icon ="",
                    Title = "Local",
                    View = typeof(LocalMainPage)
                },
                new MenuItem
                {
                    Icon = "",
                    Title = "Reddit",
                    View = typeof(RedditMainPage)
                },
            };

            SelectedMenuItem = MenuItems.FirstOrDefault();

        }

        internal void SetAppFrame(Frame viewFrame)
        {
            appFrame = viewFrame;
        }

        internal void SetSplitFrame(Frame viewFrame)
        {
            splitViewFrame = viewFrame;
        }
    }
}
