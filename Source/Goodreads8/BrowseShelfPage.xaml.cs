using System;
using System.Collections.Generic;
using System.IO;
using Goodreads8.Common;
using Goodreads8.ViewModel;
using Goodreads8.ViewModel.Model;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Goodreads8
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BrowseShelfPage : Page
    {
        private ShelfViewModel model;
        private IncrementalShelf.ShelfArguments args = null;
        private IncrementalSource<IncrementalShelf, Review> source = null;

        public class Args
        {
            public String ShelfName { get; set; }
            public int UserId { get; set; }
        }

        public BrowseShelfPage()
        {
            this.InitializeComponent();
            args = new IncrementalShelf.ShelfArguments();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Args arg = e.Parameter as Args;
            if (arg == null || string.IsNullOrEmpty(arg.ShelfName) || arg.UserId <= 0)
            {
                this.Frame.GoBack();
            }

            args.name = arg.ShelfName;
            args.sort = "date_added";
            args.order = "d";

            model = new ShelfViewModel();
            model.ShelfName = arg.ShelfName;
            model.IsBusy = true;
            this.DataContext = model;
            args.userId = arg.UserId;

            LoadData();
        }

        void source_BeginLoad()
        {
            model.IsBusy = true;
        }

        void source_EndLoad()
        {
            model.IsBusy = false;
        }

        private void LoadData()
        {
            if (source == null)
            {
                source = new IncrementalSource<IncrementalShelf, Review>(args);
                source.EndLoad += source_EndLoad;
                source.BeginLoad += source_BeginLoad;
                this.gv.ItemsSource = source;
            }
            else
            {
                source.UpdateArgument(args);
                source.LoadMoreItemsAsync(1);
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

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Review r = e.ClickedItem as Review;
            this.Frame.Navigate(typeof(BookDetailPage), r.Book.Id);
        }

        private async void Sort_Click(object sender, RoutedEventArgs e)
        {
            var popupMenu = new PopupMenu();


            string[] options = new string[] { "title", "author", "rating", "date_pub", "date_read", "avg_rating" };
            string[] optionsDisplay = new string[] { "Title", "Author", "My Rating", "Date Published","Date Added", "Average Rating"};
            string[] defaultOrder = new string[] { "a", "a", "d", "d", "d", "d" };

            for(int x=0; x<options.Length; x++)
            {
                var cmd = new UICommand(optionsDisplay[x]);
                cmd.Id = x;
                popupMenu.Commands.Add(cmd);
            }

            var button = (Button)sender;
            var transform = button.TransformToVisual(this);
            var point = transform.TransformPoint(new Point(45, -10));

            IUICommand result = await popupMenu.ShowAsync(point);

            int position = (int)result.Id;

            args.sort = options[position];
            args.order = defaultOrder[position];
            LoadData();

        }

        private void Direction_Click(object sender, RoutedEventArgs e)
        {
            if (args.order.Equals("a"))
                args.order = "d";
            else
                args.order = "a";

            LoadData();
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
