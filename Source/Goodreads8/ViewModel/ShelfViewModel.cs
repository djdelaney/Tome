using Goodreads8.Common;
using Goodreads8.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel
{
    class ShelfViewModel : BindableBase
    {
        public ShelfViewModel()
        {
            source = null;
        }

        private bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            set { isBusy = value; OnPropertyChanged("IsBusy"); }
        }

        private String shelfName;
        public String ShelfName
        {
            get { return shelfName; }
            set { shelfName = value; OnPropertyChanged("ShelfName"); }
        }

        private IncrementalSource<IncrementalShelf, Review> source;
        public IncrementalSource<IncrementalShelf, Review> Source
        {
            get { return source; }
            set { source = value; OnPropertyChanged("Source"); }
        }
    }
}
