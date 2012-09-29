using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel.Model
{
    class Topic
    {
        public int Id { get; set; }
        public String Title { get; set; }
        public DateTime LastCommentAt { get; set; }
        public int CommentCount { get; set; }
        public List<Comment> Comments { get; set; }
        public String Author { get; set; }

        public class Comment
        {
            public int Id { get; set; }
            public String Body { get; set; }
            public Profile User { get; set; }
            public DateTime UpdatedAt { get; set; }

            public String ListViewText
            {
                get
                {
                    if (User == null || string.IsNullOrEmpty(User.Name))
                        return "Unknown";

                    if (UpdatedAt == null)
                        return User.Name;

                    return User.Name + ", " + UpdatedAt.ToString("MMMM dd, h:mm tt");
                }
            }
        }

        public String ListViewText
        {
            get
            {
                String user = Author;
                if (String.IsNullOrEmpty(user))
                    user = "Unknown";

                if (LastCommentAt == null)
                    return user;

                return user + ", " + LastCommentAt.ToString("MMMM dd, h:mm tt");
            }
        }

        public String CommentCountText
        {
            get
            {
                return CommentCount.ToString() + " post(s)";
            }
        }


    }
}
