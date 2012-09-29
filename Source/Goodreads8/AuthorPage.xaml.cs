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
    public sealed partial class AuthorPage : Page
    {
        private Author model;

        public AuthorPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
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
            int? authorId = e.Parameter as int?;

            if (authorId == null)
            {
                this.Frame.GoBack();
            }

            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.busyRing.IsActive = true;

            GoodreadsAPI api = GoodreadsAPI.Instance;
            model = await api.GetAuthor((int)authorId);
            this.DataContext = model;

            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.busyRing.IsActive = false;
        }

        private void Book_Click(object sender, ItemClickEventArgs e)
        {
            Book b = e.ClickedItem as Book;
            this.Frame.Navigate(typeof(BookDetailPage), b.Id);
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

        private void AuthorWorks_Click(object sender, RoutedEventArgs e)
        {
            BrowseAuthorBooks.AuthorBooksArgs arg = new BrowseAuthorBooks.AuthorBooksArgs();
            arg.AuthorId = model.Id;
            arg.AuthorName = model.Name;
            this.Frame.Navigate(typeof(BrowseAuthorBooks), arg);
        }
    }
}
