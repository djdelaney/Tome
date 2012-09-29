using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel.Model
{
    public class Profile
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String ImageUrl { get; set; }
        public int Age { get; set; }
        public String Gender { get; set; }
        public String Location { get; set; }
        public DateTime Joined { get; set; }
        public String Link { get; set; }
        public List<Shelf> Shelves { get; set; }
        public List<Author> FavoriteAuthors { get; set; }
        public String FavoriteBooks { get; set; }
        public List<Update> Updates { get; set; }
        public String Interests { get; set; }
        public String About { get; set; }
        public String Website { get; set; }

        public String JoinedText
        {
            get
            {
                if (Joined == null)
                    return "Unknown";

                return Joined.ToString("MMMM yyyy");
            }
        }

        public String AgeText
        {
            get
            {
                if (Age == 0)
                    return "Unknown";

                return Age.ToString();
            }
        }

        public String BooksText
        {
            get
            {
                if (String.IsNullOrEmpty(FavoriteBooks))
                    return "None listed";

                return FavoriteBooks;
            }
        }

        public String AuthorsText
        {
            get
            {
                if (FavoriteAuthors == null || FavoriteAuthors.Count == 0)
                    return "None";

                String txt = "";
                foreach (Author a in FavoriteAuthors)
                {
                    if(txt.Length != 0)
                        txt = txt + ", ";

                    txt = txt + a.Name;
                }

                return txt;
            }
        }
    }
}
