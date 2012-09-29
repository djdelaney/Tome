using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel.Model
{
    public class Status
    {
        public int Id { get; set; }
        public String Header { get; set; }
        public String Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LikeCount { get; set; }
        public Profile User { get; set; }

        public String DateText
        {
            get
            {
                return CreatedAt.ToString("MMMM dd, h:mm tt");
            }
        }


        public List<Comment> Comments { get; set; }
        public class Comment
        {
            public int Id { get; set; }
            public String Body { get; set; }
            public Profile User { get; set; }
            public DateTime UpdatedAt { get; set; }

            public String DateText
            {
                get
                {
                    return UpdatedAt.ToString("MMMM dd, h:mm tt");
                }
            }
        }
    }
}
