using AutoSlide.Views;
using System;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using AutoSlide.ViewModels;

namespace AutoSlide
{
    public class NavLink
    {
        public String Label { get; set; }
        public Type LinkType { get; set; }
        public override String ToString()
        {
            return Label;
        }
    }

    public sealed partial class MainPage : Page
    {
        public List<NavLink> NavLinks = new List<NavLink>()
        {
            new NavLink() { Label = "Local", LinkType = typeof(LocalMainPage) },
            new NavLink() { Label = "Reddit", LinkType = typeof(RedditMainPage) }
        };

        public MainPage()
        {
            // Maintain state between pages
            //this.NavigationCacheMode = NavigationCacheMode.Enabled;

            this.InitializeComponent();
            //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            //SystemNavigationManager.GetForCurrentView().BackRequested += PageBackRequested;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            MainViewModel mvm = new MainViewModel();
            mvm.SetSplitFrame(SplitViewFrame);
            mvm.InitialMenuItems();
            rootGrid.DataContext = mvm;
            //mainSplitView.IsPaneOpen = false;
        }

        private void SplitViewToggle_Click(object sender, RoutedEventArgs e)
        {
            mainSplitView.IsPaneOpen = !mainSplitView.IsPaneOpen;
        }

        private void PageBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (SplitViewFrame == null)
                return;
            if (SplitViewFrame.CanGoBack)
            {
                e.Handled = true;
                SplitViewFrame.GoBack();
            }
        }
    }
}
