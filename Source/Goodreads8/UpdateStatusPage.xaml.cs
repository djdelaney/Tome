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
    public sealed partial class UpdateStatusPage : Page
    {
        private Book m_book;
        public UpdateStatusPage()
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
            int? bookId = e.Parameter as int?;
            if (bookId == null || bookId <= 0)
            {
                this.Frame.GoBack();
            }

            SaveButton.IsEnabled = false;
            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.busyRing.IsActive = true;

            GoodreadsAPI api = GoodreadsAPI.Instance;
            m_book = await api.GetBook((int)bookId);

            if (m_book == null)
            {
                ShowSimpleToast("Unable to load book data");
                Frame.GoBack();
                return;
            }

            //Review myReview = await api.GetUserReview((int)bookId, api.AuthenticatedUserId);
            //Page_Checked(null, null);

            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.busyRing.IsActive = false;
            SaveButton.IsEnabled = true;
            Page_Checked(null, null);
        }

        private void Page_Checked(object sender, RoutedEventArgs e)
        {
            if (this.statusTextBefore == null || busyRing.IsActive)
                return;

            this.statusTextBefore.Text = "I'm on page";
            this.statusTextBox.Text = "0";
            this.statusTextAfter.Text = "of " + m_book.Title;
        }

        private void Percent_Checked(object sender, RoutedEventArgs e)
        {
            if (this.statusTextBefore == null || busyRing.IsActive)
                return;

            this.statusTextBefore.Text = "I'm";
            this.statusTextBox.Text = "0";
            this.statusTextAfter.Text = "percent done " + m_book.Title;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.busyRing.IsActive)
                return;

            SaveButton.IsEnabled = false;
            GoodreadsAPI api = GoodreadsAPI.Instance;

            int number = 0;
            if (!Int32.TryParse(statusTextBox.Text, out number))
            {
                ShowSimpleToast("You must enter a valid number");
                SaveButton.IsEnabled = true;
                return;
            }

            bool usePercent = (bool)percentCheck.IsChecked;
            if (usePercent && (number <= 0 || number >= 100))
            {
                ShowSimpleToast("You must enter a valid percentage");
                SaveButton.IsEnabled = true;
                return;
            }

            if (!usePercent && number <= 0)
            {
                ShowSimpleToast("You must enter a valid page");
                SaveButton.IsEnabled = true;
                return;
            }

            bool success = await api.PostStatusUpdate(m_book.Id,
                StatusText.Text,
                usePercent ? 0 : number,
                usePercent ? number : 0);

            if (success)
                Frame.GoBack();
            else
            {
                ShowSimpleToast("Unable to post a status update. Try again later");
                SaveButton.IsEnabled = true;
            }
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
