using Goodreads8.ViewModel;
using Goodreads8.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public sealed partial class ManageShelvesPage : Goodreads8.Common.LayoutAwarePage
    {
        public ManageShelvesPage()
        {
            this.InitializeComponent();
            Window.Current.SizeChanged += WindowSizeChanged;
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

        Review myReview = null;
        Profile me = null;
        int ReviewingBookId = 0;
        private ObservableCollection<Shelf> shelfModel;

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
            base.OnNavigatedTo(e);
            int? bookId = e.Parameter as int?;
            if (bookId == null || bookId <= 0)
            {
                this.Frame.GoBack();
            }
            ReviewingBookId = (int)bookId;

            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.busyRing.IsActive = true;

            GoodreadsAPI api = GoodreadsAPI.Instance;
            me = await api.GetProfile(api.AuthenticatedUserId);

            if (me == null)
            {
                await UIUtil.ShowError("Unable to load information from Goodreads. Please try again later");
                Frame.GoBack();
                return;
            }

            myReview = await api.GetUserReview((int)bookId, api.AuthenticatedUserId);

            shelfModel = new ObservableCollection<Shelf>();
            List<Shelf> toSelect = new List<Shelf>();
            foreach (Shelf s in me.Shelves)
            {
                //Only custom shelves
                if (s.Name.Equals("read") || s.Name.Equals("to-read") || s.Name.Equals("currently-reading"))
                    continue;

                shelfModel.Add(s);
                if (myReview != null && myReview.Shelves.Contains(s.Name))
                {
                    toSelect.Add(s);
                }
            }
            this.Shelves.ItemsSource = shelfModel;

            foreach (Shelf s in toSelect)
                this.Shelves.SelectedItems.Add(s);

            //Main SHELF
            if (myReview != null && myReview.Shelves != null)
            {
                if (myReview.Shelves.Contains("read"))
                    MainShelf.SelectedIndex = 2;
                else if (myReview.Shelves.Contains("currently-reading"))
                    MainShelf.SelectedIndex = 1;
            }

            this.busyGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.busyRing.IsActive = false;

        }

        private async void SaveClick(object sender, RoutedEventArgs e)
        {
            if (this.busyRing.IsActive)
                return;

            SaveButton.IsEnabled = false;

            //Original shelves
            List<String> originalShelves = new List<String>();
            if (myReview != null && myReview.Shelves != null)
            {
                foreach (String s in myReview.Shelves)
                    originalShelves.Add(s);
            }

            //New shelves
            List<String> newShelves = new List<String>();
            ComboBoxItem cbi = MainShelf.SelectedValue as ComboBoxItem;
            String mainShelf = cbi.Content as String;
            newShelves.Add(mainShelf.ToLower());

            foreach (Shelf sh in Shelves.SelectedItems)
            {
                newShelves.Add(sh.Name);
            }


            //To modify
            List<String> toAdd = newShelves.Except(originalShelves).ToList();
            List<String> toRemove = originalShelves.Except(newShelves).ToList();

            //if(toAdd != null)
            GoodreadsAPI api = GoodreadsAPI.Instance;
            foreach (String shelf in toAdd)
            {
                bool success = await api.AddToShelf(ReviewingBookId, shelf);
                if (!success)
                {
                    await new MessageDialog("Unable to update Goodreads. Try again later").ShowAsync();
                    this.Frame.GoBack();
                }
            }
            foreach (String shelf in toRemove)
            {
                //Dont call remove on a main shelf
                if (shelf.Equals("to-read") || shelf.Equals("currently-reading") || shelf.Equals("read"))
                    continue;

                bool success = await api.RemoveFromShelf(ReviewingBookId, shelf);
                if (!success)
                {
                    await new MessageDialog("Unable to update Goodreads. Try again later").ShowAsync();
                    this.Frame.GoBack();
                }
            }

            this.Frame.GoBack();
        }
    }
}
