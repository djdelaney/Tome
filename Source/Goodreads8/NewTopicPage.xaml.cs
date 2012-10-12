using Goodreads8.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Goodreads8
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class NewTopicPage : Goodreads8.Common.LayoutAwarePage
    {
        public class Args
        {
            public int GroupId { get; set; }
            public int FolderId { get; set; }
        }

        private Args m_args = null;

        public NewTopicPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            m_args = e.Parameter as Args;
            if (m_args == null || m_args.GroupId <= 0 || m_args.FolderId <= 0)
            {
                this.Frame.GoBack();
            }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private async void PostButton_Click(object sender, RoutedEventArgs e)
        {
            if (busyRing.IsActive)
                return;

            if (string.IsNullOrEmpty(CommentTitle.Text))
            {
                UIUtil.ShowToast("You must specify a title");
                return;
            }
            if (string.IsNullOrEmpty(CommentBox.Text))
            {
                UIUtil.ShowToast("You must specify a comment");
                return;
            }

            this.PostButton.IsEnabled = false;
            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.busyRing.IsActive = true;

            GoodreadsAPI api = GoodreadsAPI.Instance;
            if (false == await api.PostNewTopic(m_args.GroupId, m_args.FolderId, CommentTitle.Text, CommentBox.Text))
            {
                await UIUtil.ShowError("Unable to post your new topic to Goodreads. Please try again later");
                this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.busyRing.IsActive = false;
                this.PostButton.IsEnabled = true;
            }
            else
            {
                this.Frame.GoBack();
            }
            
        }
    }
}
