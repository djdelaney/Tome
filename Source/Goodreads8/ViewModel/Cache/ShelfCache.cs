using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel.Cache
{
    class ShelfCache
    {
        public DateTime Expiration { get; set; }
        public int      UserId { get; set; }
        public String   Shelf { get; set; }
        public int      Page { get; set; }
        public String   Sort { get; set; }
        public String   Order { get; set; }

        public ShelfCache()
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
