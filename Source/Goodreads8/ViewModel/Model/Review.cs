using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel.Model
{
    public class Review
    {
        public String ReviewerText
        {
            get
            {
                if (Reviewer == null)
                    return "Unknown";

                if (ReadAt == null || ReadAt == DateTime.MinValue)
                    return Reviewer.Name;

                String value =  Reviewer.Name + ", " + ReadAt.ToString("MMM dd, yyyy");
                return value;
            }
        }

        public String ReviewDate
        {
            get
            {
                if (ReadAt == null || ReadAt == DateTime.MinValue)
                    return "Unknown";

                return ReadAt.ToString("MMM dd, yyyy");
            }
        }

        public String BodyText
        {
            get
            {
                if (string.IsNullOrEmpty(Body) || Body.Equals("\0"))
                {
                    if (Shelves != null && Shelves.Contains("currently-reading"))
                        return "Currently-Reading";

                    if (Shelves == null || Shelves.Contains("to-read"))
                        return "Marked To-Read";

                    if(Rating > 0)
                        return "No text";

                    return "No review";
                }

                return Body;
            }
        }

        public String ShelfText
        {
            get
            {
                if (Shelves == null || Shelves.Count == 0)
                    return "Unknown";

                return Shelves[0];
            }
        }

        public String Url { get; set; }
        public String Body { get; set; }
        public bool Spoilers { get; set; }
        public int Rating { get; set; }
        public int Id { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime ReadAt { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateUpdated { get; set; }
        public Book Book { get; set; }
        public List<String> Shelves { get; set; }
        public Profile Reviewer { get; set; }

        public int CurrentPage { get; set; }
        public int CurrentPercent { get; set; }

        public List<Comment> Comments { get; set; }

        public class Comment
        {
            public int Id { get; set; }
            public String Body { get; set; }
            public Profile User { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
}
