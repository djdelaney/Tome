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
        }

        private bool isBusy;

        public bool IsBusy
        {
            get { return isBusy; }
            set { isBusy = value; OnPropertyChanged("IsBusy"); }
        }

        public String ShelfName { get; set; }


        private ObservableCollection<Book> _books;

        public ObservableCollection<Book> Books
        {
            get
            {
                if (_books == null)
                {
                    LoadData();//Don't await...
                }
                return _books;

            }
            private set
            {
                SetProperty(ref _books, value);
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

                Books = new ObservableCollection<Book>();

                Author osc = new Author();
                osc.Name = "Orson Scott Card";
                List<Author> b1Authors = new List<Author>();
                b1Authors.Add(osc);

                Book b = new Book();
                b.Title = "Ender's Game";
                b.Authors = b1Authors;
                b.ImageUrl = "http://www.goodreads.com/assets/nocover/111x148.png";
                Books.Add(b);

                Book b1 = new Book();
                b1.Title = "Dune";
                b1.ImageUrl = "http://photo.goodreads.com/books/1332376530m/13542606.jpg";
                Books.Add(b1);
                Books.Add(b1);
                Books.Add(b1);
                Books.Add(b1);
                Books.Add(b1);
                Books.Add(b1);
                Books.Add(b1);
                Books.Add(b1);
                Books.Add(b1);
                Books.Add(b1);

                Books.Add(b1);
                Books.Add(b1);
                Books.Add(b1);
            }
            IsLoading = false;
        }
    }
}
