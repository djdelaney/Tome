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
    public sealed partial class UserPage : Goodreads8.Common.LayoutAwarePage
    {
        private Profile model;

        public UserPage()
        {
            this.InitializeComponent();
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

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            int? userId = e.Parameter as int?;

            if (userId == null)
            {
                this.Frame.GoBack();
            }

            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.busyRing.IsActive = true;

            GoodreadsAPI api = GoodreadsAPI.Instance;
            model = await api.GetProfile((int)userId);
            this.DataContext = model;

            if (model == null)
            {
                await UIUtil.ShowError("Unable to load user information from Goodreads. Please try again later");
                Frame.GoBack();
                return;
            }

            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.busyRing.IsActive = false;
        }

        private async void ClickWebsite(object sender, TappedRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(model.Website))
                return;

            try
            {
                var uri = new Uri(model.Website);
                await Windows.System.Launcher.LaunchUriAsync(uri);
            }
            catch (Exception)
            { }
        }

        private void Shelf_ItemClick(object sender, ItemClickEventArgs e)
        {
            Shelf clicked = e.ClickedItem as Shelf;

            BrowseShelfPage.Args arg = new BrowseShelfPage.Args();
            arg.UserId = model.Id;
            arg.ShelfName = clicked.Name;

            this.Frame.Navigate(typeof(BrowseShelfPage), arg);
        }


        private void Update_Click(object sender, ItemClickEventArgs e)
        {
            Update u = e.ClickedItem as Update;
            if (u.Type == Update.Actions.review)
            {
                String link = u.Link;
                if (string.IsNullOrEmpty(link) || !link.StartsWith("http://www.goodreads.com/review/show/"))
                    return;

                String parse = link.Replace("http://www.goodreads.com/review/show/", "");
                int reviewId = int.Parse(parse);

                this.Frame.Navigate(typeof(ViewReviewPage), reviewId);
            }
            else if (u.Type == Update.Actions.comment)
            {
                if (string.IsNullOrEmpty(u.Link) || !u.Link.StartsWith("http://www.goodreads.com/topic/show/"))
                    return;

                String parse = u.Link.Replace("http://www.goodreads.com/topic/show/", "");
                int pos = parse.IndexOf('-');
                if (pos > 0)
                    parse = parse.Substring(0, pos);

                int topicId = int.Parse(parse);

                this.Frame.Navigate(typeof(TopicPage), topicId);
            }
            else if (u.Type == Update.Actions.userstatus)
            {
                if (string.IsNullOrEmpty(u.Link) || !u.Link.StartsWith("http://www.goodreads.com/user_status/show/"))
                    return;

                String parse = u.Link.Replace("http://www.goodreads.com/user_status/show/", "");
                int statusId = int.Parse(parse);

                this.Frame.Navigate(typeof(ViewStatusPage), statusId);
            }
        }
    }
}
