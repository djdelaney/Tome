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
    public sealed partial class TopicPage : Page
    {
        private Topic model;
        public TopicPage()
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
            int? topicId = e.Parameter as int?;
            if (topicId == null)
            {
                this.Frame.GoBack();
            }

            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.busyRing.IsActive = true;

            GoodreadsAPI api = GoodreadsAPI.Instance;
            model = await api.GetTopic((int)topicId);
            this.DataContext = model;

            if (model == null)
            {
                await UIUtil.ShowError("Unable to load topic information from Goodreads. Please try again later");
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

        private async void PostButton_Click(object sender, RoutedEventArgs e)
        {
            if (busyRing.IsActive || model == null || string.IsNullOrEmpty(CommentBox.Text))
                return;

            this.PostButton.IsEnabled = false;

            GoodreadsAPI api = GoodreadsAPI.Instance;
            if (false == await api.PostComment(model.Id, GoodreadsAPI.CommentType.topic, CommentBox.Text))
            {
                return;
            }

            //Reload review
            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.busyRing.IsActive = true;

            model = await api.GetTopic((int)model.Id);
            if (model == null)
            {
                this.Frame.GoBack();
                return;
            }
            this.DataContext = model;

            CommentBox.Text = "";

            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.busyRing.IsActive = false;

            this.PostButton.IsEnabled = true;
        }
    }
}
