using Goodreads8.Common;
using Goodreads8.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel
{
    public class IncrementalShelf : IPagedSource<Review>
    {
        private ShelfArguments m_shelfDetails;

        public void SetArguments(Object argument)
        {
            m_shelfDetails = argument as ShelfArguments;
        }

        public async Task<IPagedResponse<Review>> GetPage(int pageIndex)
        {
        if (pageIndex < 1)
            throw new ArgumentOutOfRangeException("pageIndex");

        GoodreadsAPI api = GoodreadsAPI.Instance;
        ReviewSet page = await api.GetShelfContents(m_shelfDetails.userId, m_shelfDetails.name, pageIndex, m_shelfDetails.sort, m_shelfDetails.order, 50);

        return new ReviewResponse(page.Reviews, page.End, page.Total);
        }

        public class ShelfArguments
        {
            public String name { get; set; }
            public String sort { get; set; }
            public String order { get; set; }
            public int userId { get; set; }
        }
    }

    public class ReviewResponse : IPagedResponse<Review>
    {
        public ReviewResponse(IEnumerable<Review> items, int currentTotal, int virtualTotal)
        {
            this.Items = items;
            this.VirtualTotal = virtualTotal;
            this.CurrentTotal = currentTotal;
        }

        public int VirtualTotal { get; private set; }
        public int CurrentTotal { get; private set; }

        public IEnumerable<Review> Items { get; private set; }
    }
}
