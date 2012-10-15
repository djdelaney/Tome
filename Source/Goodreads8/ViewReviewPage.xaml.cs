using Goodreads8.ViewModel;
using Goodreads8.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
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
    public sealed partial class ViewReviewPage : Goodreads8.Common.LayoutAwarePage
    {
        private Review model;

        public ViewReviewPage()
        {
            this.InitializeComponent();
            Window.Current.SizeChanged += WindowSizeChanged;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void WindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            // Obtain view state by explicitly querying for it
            ApplicationViewState myViewState = ApplicationView.Value;
            if (ApplicationView.Value == ApplicationViewState.Snapped)
            {
                Window.Current.SizeChanged -= WindowSizeChanged;
                if (this.Frame.CanGoBack)
                    this.Frame.GoBack();
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            int? reviewId = e.Parameter as int?;

            if (reviewId == null)
            {
                this.Frame.GoBack();
            }

            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.busyRing.IsActive = true;

            GoodreadsAPI api = GoodreadsAPI.Instance;
            model = await api.GetReview((int)reviewId);
            this.DataContext = model;

            if (model == null)
            {
                await UIUtil.ShowError("Unable to load review information from Goodreads. Please try again later");
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

        private void More_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BookDetailPage), model.Book.Id);
        }

        private void Reviewer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserPage), model.Reviewer.Id);
        }

        private void Comment_Click(object sender, ItemClickEventArgs e)
        {
            Review.Comment c = e.ClickedItem as Review.Comment;
            this.Frame.Navigate(typeof(UserPage), c.User.Id);
        }

        private async void PostButton_Click(object sender, RoutedEventArgs e)
        {
            if (busyRing.IsActive || model == null || string.IsNullOrEmpty(CommentBox.Text))
                return;

            this.PostButton.IsEnabled = false;

            GoodreadsAPI api = GoodreadsAPI.Instance;
            if (false == await api.PostComment(model.Id, GoodreadsAPI.CommentType.review, CommentBox.Text))
            {
                ShowSimpleToast("Unable to post a new comment. Try again later");
                return;
            }

            //Reload review
            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.busyRing.IsActive = true;

            model = await api.GetReview((int)model.Id);
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

        void ShowSimpleToast(string message)
        {
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            var textXml = toastXml.GetElementsByTagName("text");
            textXml.Item(0).InnerText = message;

            // Create a toast from the Xml, then create a ToastNotifier object to show the toast.
            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private async void Author_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (model.Book.Authors == null)
                return;

            if (model.Book.Authors.Count == 1)
            {
                this.Frame.Navigate(typeof(AuthorPage), model.Book.Authors[0].Id);
                return;
            }

            var popupMenu = new PopupMenu();

            foreach (Author a in model.Book.Authors)
            {
                UICommand cmd = new UICommand(a.Name);
                cmd.Id = a;
                popupMenu.Commands.Add(cmd);
            }

            var button = (FrameworkElement)sender;
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

        private void Book_Cover_Tapped(object sender, TappedRoutedEventArgs e)
        {
            More_Click(sender, e);
        }
    }
}
