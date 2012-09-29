using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel.Cache
{
    class UserReviewCache
    {
        public DateTime Expiration { get; set; }
        public int      UserId { get; set; }
        public int      BookId { get; set; }

        public UserReviewCache()
        {
            Expiration = DateTime.Now.AddHours(1);
        }

        public bool IsExpired()
        {
            if (Expiration.CompareTo(DateTime.Now) < 0)
                return true;
            return false;
        }
    }
}
