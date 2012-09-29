using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel.Cache
{
    class FriendCache
    {
        public DateTime Expiration { get; set; }
        public int      UserId { get; set; }
        public int      Page { get; set; }

        public FriendCache()
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
