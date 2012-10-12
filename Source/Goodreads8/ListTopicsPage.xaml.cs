using Goodreads8.Common;
using Goodreads8.ViewModel;
using Goodreads8.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public sealed partial class ListTopicsPage : Page
    {
        public class TopicArgs
        {
            public int FolderId { get; set; }
            public int GroupId { get; set; }
            public String Name { get; set; }
        }

        private TopicArgs m_arg = null;
        private IncrementalSource<IncrementalTopics, Topic> source = null;

        public ListTopicsPage()
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
            m_arg = e.Parameter as TopicArgs;
            if (m_arg == null)
            {
                this.Frame.GoBack();
                return;
            }

            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.busyRing.IsActive = true;

            pageTitle.Text = m_arg.Name;

            IncrementalTopics.TopicArguments sourceArgs = new IncrementalTopics.TopicArguments();
            sourceArgs.GroupId = m_arg.GroupId;
            sourceArgs.FolderId = m_arg.FolderId;
            source = new IncrementalSource<IncrementalTopics, Topic>(sourceArgs);
            source.CollectionChanged += source_CollectionChanged;

            this.TopicList.ItemsSource = source;
        }

        private void TopicClick(object sender, ItemClickEventArgs e)
        {
            Topic t = e.ClickedItem as Topic;
            this.Frame.Navigate(typeof(TopicPage), t.Id);
        }

        void source_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.busyRing.IsActive = false;
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            NewTopicPage.Args arg = new NewTopicPage.Args();
            arg.FolderId = m_arg.FolderId;
            arg.GroupId = m_arg.GroupId;
            this.Frame.Navigate(typeof(NewTopicPage), arg);
        }
    }
}
