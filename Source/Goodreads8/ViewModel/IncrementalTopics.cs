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
    class IncrementalTopics : IPagedSource<Topic>
    {
        public class TopicArguments
        {
            public int GroupId { get; set; }
            public int FolderId { get; set; }
        }

        private TopicArguments args;
        public void SetArguments(Object argument)
        {
            args = argument as TopicArguments;
        }

        public async Task<IPagedResponse<Topic>> GetPage(int pageIndex)
        {
            if (pageIndex < 1)
                throw new ArgumentOutOfRangeException("pageIndex");


            GoodreadsAPI api = GoodreadsAPI.Instance;
            TopicSet set = await api.GetTopics(args.GroupId, args.FolderId, pageIndex);

            return new TopicResponse(set.Topics, set.End, set.Total);
        }

        public class TopicResponse : IPagedResponse<Topic>
        {
            public TopicResponse(IEnumerable<Topic> items, int currentTotal, int virtualTotal)
            {
                this.Items = items;
                this.VirtualTotal = virtualTotal;
                this.CurrentTotal = currentTotal;
            }

            public int VirtualTotal { get; private set; }
            public int CurrentTotal { get; private set; }

            public IEnumerable<Topic> Items { get; private set; }
        }
    }
}
