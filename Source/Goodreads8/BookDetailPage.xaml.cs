using Goodreads8.ViewModel;
using Goodreads8.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
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
    public sealed partial class BookDetailPage : Page
    {
        private Book model;

        public BookDetailPage()
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
            int? bookId = e.Parameter as int?;

            if (bookId == null)
            {
                this.Frame.GoBack();
            }

            this.ButtonStatus.IsEnabled = false;
            this.ButtonReview.IsEnabled = true;


            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.busyRing.IsActive = true;

            GoodreadsAPI api = GoodreadsAPI.Instance;
            model = await api.GetBook((int)bookId);

            Review myReview = await api.GetUserReview((int)bookId, api.AuthenticatedUserId);
            if (myReview == null || myReview.Shelves == null || myReview.Shelves.Count == 0)
            {
                shelfLabel.Text = "None";
            }
            else
            {
                shelfLabel.Text = myReview.ShelfText;
                if(myReview.Shelves.Contains("currently-reading"))
                    this.ButtonStatus.IsEnabled = true;
                if (myReview.Shelves.Contains("read"))
                    this.ButtonReview.IsEnabled = false;
            }

            this.DataContext = model;

            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.busyRing.IsActive = false;
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

        private void Review_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(WriteReviewPage));
        }

        private void Status_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UpdateStatusPage));
        }

        private async void Author_Click(object sender, RoutedEventArgs e)
        {
            if (model.Authors == null)
                return;

            if (model.Authors.Count == 1)
            {
                this.Frame.Navigate(typeof(AuthorPage), model.Authors[0].Id);
                return;
            }

            var popupMenu = new PopupMenu();

            foreach (Author a in model.Authors)
            {
                UICommand cmd = new UICommand(a.Name);
                cmd.Id = a;
                popupMenu.Commands.Add(cmd);
            }

            var button = (Button)sender;
            var transform = button.TransformToVisual(this);
            var point = transform.TransformPoint(new Point(45, -10));

            IUICommand result = await popupMenu.ShowAsync(point);

            Author toView = result.Id as Author;
            if (toView != null)
            {
                this.Frame.Navigate(typeof(AuthorPage), toView.Id);
                    return;
            }
        }

        private void Shelf_Click(object sender, RoutedEventArgs e)
        {
            if(model != null)
                this.Frame.Navigate(typeof(ManageShelvesPage), model.Id);
        }

        private void Review_ItemClick(object sender, ItemClickEventArgs e)
        {
            Review clicked = e.ClickedItem as Review;
            if (clicked.Shelves == null || !clicked.Shelves.Contains("read"))
                return;

            this.Frame.Navigate(typeof(ViewReviewPage), clicked.Id);
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
