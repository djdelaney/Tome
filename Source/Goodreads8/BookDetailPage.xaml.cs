﻿using Goodreads8.ViewModel;
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
    public sealed partial class BookDetailPage : Goodreads8.Common.LayoutAwarePage
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
            base.OnNavigatedTo(e);
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

            if (model == null)
            {
                await UIUtil.ShowError("Unable to load book information from Goodreads. Please try again later");
                Frame.GoBack();
                return;
            }

            if (model.MyReview != null && model.MyReview.Shelves.Count > 0)
            {
                if (model.MyReview.Shelves.Contains("currently-reading"))
                    this.ButtonStatus.IsEnabled = true;
                if (model.MyReview.Shelves.Contains("read"))
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
            if (model != null && model.Id > 0)
                this.Frame.Navigate(typeof(WriteReviewPage), new WriteReviewPage.Args(model.Id, 0, model.Title));
        }

        private void Status_Click(object sender, RoutedEventArgs e)
        {
            if (model != null && model.Id > 0)
                this.Frame.Navigate(typeof(UpdateStatusPage), model.Id);
        }

        PopupMenu popupMenu = null;
        private async void Author_Click(object sender, RoutedEventArgs e)
        {
            if (model == null || model.Authors == null)
                return;

            if (model.Authors.Count == 1)
            {
                this.Frame.Navigate(typeof(AuthorPage), model.Authors[0].Id);
                return;
            }

            if (popupMenu != null)
                return;

            popupMenu = new PopupMenu();

            foreach (Author a in model.Authors)
            {
                UICommand cmd = new UICommand(a.Name);
                cmd.Id = a;
                popupMenu.Commands.Add(cmd);
            }

            var button = (FrameworkElement)sender;
            var transform = button.TransformToVisual(this);
            var point = transform.TransformPoint(new Point(45, -10));

            IUICommand result = await popupMenu.ShowAsync(point);
            if (result == null)
            {
                popupMenu = null;
                return;
            }

            Author toView = result.Id as Author;
            if (toView != null)
            {
                this.Frame.Navigate(typeof(AuthorPage), toView.Id);
                    return;
            }

            popupMenu = null;
        }

        private void Shelf_Click(object sender, RoutedEventArgs e)
        {
            if(model != null && model.Id > 0)
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

        private void Author_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Author_Click(sender, e);
        }

        private void Rating_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (model == null)
                return;

            if (model.MyReview.Id > 0 && model.MyReview.Shelves != null && model.MyReview.Shelves.Contains("read"))
                this.Frame.Navigate(typeof(ViewReviewPage), model.MyReview.Id);
            else
            {
                int rating = (int)starRating.Value;
                this.Frame.Navigate(typeof(WriteReviewPage), new WriteReviewPage.Args(model.Id, rating, model.Title));
            }
        }
    }
}
