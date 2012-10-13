using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel.Model
{
    public class Book : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public String PrimaryAuthor
        {
            get
            {
                if (Authors == null || Authors.Count == 0)
                    return "Unknown Author";

                return Authors[0].Name;
            }
        }

        public String PublishedText
        {
            get
            {
                if (PublicationYear == 0)
                    return "Unknown";

                if (PublicationMonth == 0)
                    return PublicationYear.ToString();

                DateTime value = new DateTime(PublicationYear, PublicationMonth, 1);
                return value.ToString("MMMM yyyy");
            }
        }

        public int Id { get; set; }
        public String Title { get; set; }
        public String Description { get; set; }
        public String ImageUrl { get; set; }
        public String SmallImageUrl { get; set; }
        public String BookUrl { get; set; }
        public String Publisher { get; set; }
        public double AvgRating { get; set; }
        public List<Author> Authors { get; set; }
        public String ISBN { get; set; }
        public List<Review> Reviews { get; set; }
        public List<Review> FriendReviews { get; set; }
        //private Map<String, String> m_onlineStores;

        public Review MyReview { get; set; }

        public List<String> Shelves { get; set; }


        public int PublicationYear { get; set; }
        public int PublicationMonth { get; set; }
        public int PublicationDay { get; set; }
    }
}
