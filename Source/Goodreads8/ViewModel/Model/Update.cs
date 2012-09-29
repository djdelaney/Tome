using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel.Model
{
    public class Update
    {
        public enum Actions { review, userstatus, comment, rating, other, eventresponse, userquote, userlistvote }

        public Actions Type { get; set; }
        public String Text { get; set; }
        public String Link { get; set; }
        public String ImageUrl { get; set; }
        public String Body { get; set; }
        public Profile User { get; set; }
        public DateTime UpdateTime { get; set; }
        public UserStatus Status { get; set; }
        public Book Book { get; set; }

        public String UpdateText
        {
            get
            {
                if (Type == Actions.userstatus || Type == Actions.userquote)
                    return Body;
                return Text;
            }
        }

        public String UserText
        {
            get
            {
                if (User == null)
                    return "Unknown User";

                if (UpdateTime == null)
                    return User.Name;

                return User.Name + ", " + UpdateTime.ToString("MMMM dd, h:mm tt");
            }
        }
    }
}
