using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel.Cache
{
    class TopicSetCache
    {
        public DateTime Expiration { get; set; }
        public int      GroupId { get; set; }
        public int      FolderId { get; set; }
        public int      Page { get; set; }

        public TopicSetCache()
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
