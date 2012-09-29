using Goodreads8.Common;
using Goodreads8.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Goodreads8.ViewModel
{
    class MainPageViewModel : BindableBase
    {

        public String ShelfName { get; set; }

        //Shelves
        private ObservableCollection<Shelf> _shelves;
        public ObservableCollection<Shelf> Shelves
        {
            get
            {
                if (_updates == null)
                {
                    LoadData();//Don't await...
                }
                return _shelves;

            }
            private set
            {
                SetProperty(ref _shelves, value);
            }
        }

        //Updates
        private ObservableCollection<Update> _updates;
        public ObservableCollection<Update> Updates
        {
            get
            {
                if (_updates == null)
                {
                    LoadData();//Don't await...
                }
                return _updates;

            }
            private set
            {
                SetProperty(ref _updates, value);
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            private set
            {
                SetProperty(ref _isLoading, value);
            }
        }

        private async Task LoadData()
        {
            if (!IsLoading)
            {
                IsLoading = true;

                Updates = new ObservableCollection<Update>();
                GoodreadsAPI api = GoodreadsAPI.Instance;
                List<Update> updateList = await api.GetUpdates();
                foreach (Update u in updateList)
                {
                    Updates.Add(u);
                }

                //Update tiles
                MainPage.SetupTiles();

                //Shelves
                Shelves = new ObservableCollection<Shelf>();

                Profile p = await api.GetProfile(api.AuthenticatedUserId);
                int curShelf = 0;
                foreach (Shelf s in p.Shelves)
                {
                    s.BG = CreateBG(curShelf, p.Shelves.Count);
                    Shelves.Add(s);
                    curShelf++;
                }
            }
            IsLoading = false;
        }

        private Brush CreateBG(int index, int count)
        {
            int max = 255;
            int min = 80;
            int increment = (max-min)/count;

            int value = max - (increment*index);
            return new SolidColorBrush(Color.FromArgb(255, 0, (byte)value, (byte)value));
        }
    }
}
