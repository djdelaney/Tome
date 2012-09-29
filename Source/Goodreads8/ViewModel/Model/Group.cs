using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel.Model
{
    class Group
    {
        public int Id { get; set; }
        public String Title { get; set; }
        public String Access { get; set; }
        public int UserCount { get; set; }
        public String ImageUrl { get; set; }
        public DateTime LastActivity { get; set; }
        public String Category { get; set; }
        public String SubCategory { get; set; }
        public String Rules { get; set; }
        public String Link { get; set; }
        public String Description { get; set; }
        public String Location { get; set; }
        public List<Folder> Folders { get; set; }

        public class Folder
        {
            public int Id { get; set; }
            public String Name { get; set; }
            public int TopicCount { get; set; }
            public DateTime UpdatedAt { get; set; }

            public String CountText
            {
                get
                {
                    return TopicCount + " topics";
                }
            }
        }
    }
}
