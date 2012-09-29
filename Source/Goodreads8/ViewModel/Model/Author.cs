using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel.Model
{
    public class Author
    {
        public String BornText
        {
            get
            {
                if (Born == null && Hometown == null)
                    return "Unknown";

                if (Born == null)
                    return Hometown;
                else if (Hometown == null)
                    return Born.ToString("MMMM yyyy");

                return Born.ToString("MMMM yyyy") + " in " + Hometown;
            }
        }

        public String GenderText
        {
            get
            {
                if (string.IsNullOrEmpty(Gender))
                    return "Unknown";

                if (Gender.Equals("male"))
                    return "Male";
                else if (Gender.Equals("female"))
                    return "Female";

                return Gender;
            }
        }

        public String Name { get; set; }
        public int Id { get; set; }
        public int FanCount { get; set; }
        public int WorksCount { get; set; }
        public String Link { get; set; }
        public String ImageUrl { get; set; }
        public String SmallImageUrl { get; set; }
        public String About { get; set; }
        public String Influences { get; set; }
        public String Gender { get; set; }
        public String Hometown { get; set; }
        public DateTime Born { get; set; }
        public DateTime Died { get; set; }
        public List<Book> Books { get; set; }
    }
}
