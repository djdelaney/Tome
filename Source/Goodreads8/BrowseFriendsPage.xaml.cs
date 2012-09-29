using Goodreads8.Common;
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
    public sealed partial class BrowseFriendsPage : Page
    {
        private BusyViewModel model;
        private IncrementalSource<IncrementalFriends, Profile> source;

        public BrowseFriendsPage()
        {
            this.InitializeComponent();

            model = new BusyViewModel();
            model.IsBusy = true;
            this.DataContext = model;

            GoodreadsAPI api = GoodreadsAPI.Instance;

            IncrementalFriends.FriendArguments args = new IncrementalFriends.FriendArguments();
            args.userId = api.AuthenticatedUserId;

            source = new IncrementalSource<IncrementalFriends, Profile>(args);
            source.CollectionChanged += source_CollectionChanged;
            source.BeginLoad += source_BeginLoad;
            this.gv.ItemsSource = source;
        }

        void source_BeginLoad()
        {
            model.IsBusy = true;
        }

        void source_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            model.IsBusy = false ;
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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void Friend_Click(object sender, ItemClickEventArgs e)
        {
            Profile p = e.ClickedItem as Profile;
            this.Frame.Navigate(typeof(UserPage), p.Id);
        }

    }
}
