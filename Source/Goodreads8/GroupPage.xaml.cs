using Goodreads8.ViewModel;
using Goodreads8.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Goodreads8
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GroupPage : Page
    {
        private Group model;
        public GroupPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            int? groupId = e.Parameter as int?;
            if (groupId == null)
            {
                this.Frame.GoBack();
            }

            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.busyRing.IsActive = true;

            GoodreadsAPI api = GoodreadsAPI.Instance;
            model = await api.GetGroup((int)groupId);
            this.DataContext = model;

            if (model == null)
            {
                await UIUtil.ShowError("Unable to load group information from Goodreads. Please try again later");
                Frame.GoBack();
                return;
            }

            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.busyRing.IsActive = false;
        }

        /// <summary>
        /// Invoked as an event handler to navigate backward in the navigation stack
        /// associated with this page's <see cref="Frame"/>.
        /// </summary>
        /// <param name="sender">Instance that triggered the event.</param>
        /// <param name="e">Event data describing the conditions that led to the
        /// event.</param>
        public void GoBack(object sender, RoutedEventArgs e)
        {
            // Use the navigation frame to return to the previous page
            if (this.Frame != null && this.Frame.CanGoBack) this.Frame.GoBack();
        }

        private async void ClickWebsite(object sender, TappedRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(model.Link))
                return;

            try
            {
                var uri = new Uri(model.Link);
                await Windows.System.Launcher.LaunchUriAsync(uri);
            }
            catch (Exception)
            { }
        }

        private void FolderClick(object sender, ItemClickEventArgs e)
        {
            Group.Folder f = e.ClickedItem as Group.Folder;

            ListTopicsPage.TopicArgs args = new ListTopicsPage.TopicArgs();
            args.FolderId = f.Id;
            args.GroupId = model.Id;
            args.Name = f.Name;
            this.Frame.Navigate(typeof(ListTopicsPage), args);
        }
    }
}
