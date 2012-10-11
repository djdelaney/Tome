using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel.Cache
{
    class CacheEntry
    {
        public DateTime Expiration { get; set; }
        public Object   Object { get; set; }

        public CacheEntry(Object obj)
        {
            Expiration = DateTime.Now.AddHours(1);
            this.Object = obj;
        }

        public bool IsExpired()
        {
            if (Expiration.CompareTo(DateTime.Now) < 0)
                return true;
            return false;
        }
    }
}
