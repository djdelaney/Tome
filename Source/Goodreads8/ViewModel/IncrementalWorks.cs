using Goodreads8.Common;
using Goodreads8.ViewModel;
using Goodreads8.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel
{
    class IncrementalWorks : IPagedSource<Book>
    {
        public class WorksArguments
        {
            public int AuthorId { get; set; }
        }

        private WorksArguments args;
        public void SetArguments(Object argument)
        {
            args = argument as WorksArguments;
        }

        public async Task<IPagedResponse<Book>> GetPage(int pageIndex)
        {
            if (pageIndex < 1)
                throw new ArgumentOutOfRangeException("pageIndex");


            GoodreadsAPI api = GoodreadsAPI.Instance;
            BookSet set = await api.GetAuthorBooks(args.AuthorId, pageIndex);

            return new BookResponse(set.Books, set.End, set.Total);
        }

        [DebuggerDisplay("PageIndex = {PageIndex} - VirtualCount = {VirtualCount}")]
        public class BookResponse : IPagedResponse<Book>
        {
            public BookResponse(IEnumerable<Book> items, int currentTotal, int virtualTotal)
            {
                this.Items = items;
                this.VirtualTotal = virtualTotal;
                this.CurrentTotal = currentTotal;
            }

            public int VirtualTotal { get; private set; }
            public int CurrentTotal { get; private set; }

            public IEnumerable<Book> Items { get; private set; }
        }
    }
}
