using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel.Model
{
    class SearchResult : Goodreads8.Common.BindableBase
    {
        private int _id;
        private String _image;
        private String _title;
        private String _subtitle;
        private String _description;

        public int Id
        {
            get { return _id; }
            set { if (this.SetProperty(ref _id, value)) this.OnPropertyChanged("Id"); }
        }

        public String Image
        {
            get { return _image; }
            set { if (this.SetProperty(ref _image, value)) this.OnPropertyChanged("Image"); }
        }

        public String Title
        {
            get { return _title; }
            set { if (this.SetProperty(ref _title, value)) this.OnPropertyChanged("Title"); }
        }

        public String Subtitle
        {
            get { return _subtitle; }
            set { if (this.SetProperty(ref _subtitle, value)) this.OnPropertyChanged("Subtitle"); }
        }
        public String Description
        {
            get { return _description; }
            set { if (this.SetProperty(ref _description, value)) this.OnPropertyChanged("Description"); }
        }

    }
}
