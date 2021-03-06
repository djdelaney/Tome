﻿using Goodreads8.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
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
    public sealed partial class WriteReviewPage : Goodreads8.Common.LayoutAwarePage
    {
        int m_bookId = 0;
        public WriteReviewPage()
        {
            this.InitializeComponent();
            Window.Current.SizeChanged += WindowSizeChanged;
        }

        public class Args
        {
            public Args(int book, int rating, String title)
            {
                BookId = book;
                Rating = rating;
                Title = title;
            }

            public String Title { get; set; }
            public int BookId { get; set; }
            public int Rating { get; set; }
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
        /// Leaving, unregister event handlers
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Window.Current.SizeChanged -= WindowSizeChanged;
            base.OnNavigatedFrom(e);
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
            base.OnNavigatedTo(e);

            Args arg = e.Parameter as Args;
            if (arg == null || arg.BookId <= 0)
            {
                this.Frame.GoBack();
                return;
            }

            pageTitle.Text = "Review: " + arg.Title;

            if (arg.Rating > 0)
                starRating.Value = arg.Rating;
            
            m_bookId = arg.BookId;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.busyRing.IsActive)
                return;

            SaveButton.IsEnabled = false;

            if (starRating.Value <= 0)
            {
                ShowSimpleToast("You must specify a rating");
                SaveButton.IsEnabled = true;
                return;
            }

            GoodreadsAPI api = GoodreadsAPI.Instance;
            if (true != await api.PostBookReview(m_bookId, body.Text, (int)starRating.Value))
            {
                ShowSimpleToast("Unable to post your review. Try again later.");
                SaveButton.IsEnabled = true;
                return;
            }

            Frame.GoBack();
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
    }
}
