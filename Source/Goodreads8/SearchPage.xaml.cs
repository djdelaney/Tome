using Goodreads8.ViewModel;
using Goodreads8.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
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
    public sealed partial class SearchPage : Goodreads8.Common.LayoutAwarePage
    {
        List<Book> model;

        public SearchPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            Window.Current.SizeChanged += WindowSizeChanged;
        }

        private void WindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            // Obtain view state by explicitly querying for it
            ApplicationViewState myViewState = ApplicationView.Value;
            if (ApplicationView.Value == ApplicationViewState.Snapped)
            {
                this.gv.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.lv.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                this.gv.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.lv.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ApplicationView.Value == ApplicationViewState.Snapped)
            {
                this.gv.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                this.lv.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
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

        private void Book_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!query.IsEnabled)
                return;

            Book b = e.ClickedItem as Book;
            this.Frame.Navigate(typeof(BookDetailPage), b.Id);
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.query.Text))
            {
                MessageDialog msg=new MessageDialog("You must specify search terms","Search");
                await msg.ShowAsync();
                return;
            }
            model = null;
            this.gv.ItemsSource = null;
            this.lv.ItemsSource = null;

            this.SearchBtn.IsEnabled = false;
            this.query.IsEnabled = false;
            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.busyRing.IsActive = true;

            GoodreadsAPI api = GoodreadsAPI.Instance;
            model = await api.GetSearchResults(this.query.Text);
            this.gv.ItemsSource = model;
            this.lv.ItemsSource = model;

            this.SearchBtn.IsEnabled = true;
            this.query.IsEnabled = true;
            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.busyRing.IsActive = false;

            if (model == null)
                UIUtil.ShowError("Unable to load Goodreads results. Please try again later");
        }

        private void Query_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (!string.IsNullOrEmpty(this.query.Text))
                {
                    query.IsEnabled = false;
                    Search_Click(sender, e);
                }
            }
        }
    }
}
