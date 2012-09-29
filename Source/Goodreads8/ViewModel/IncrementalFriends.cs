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

    public class IncrementalFriends : IPagedSource<Profile>
    {
        public class FriendArguments
        {
            public int userId { get; set; }
        }

        private int userId;
        public void SetArguments(Object argument)
        {
            FriendArguments arg = argument as FriendArguments;
            userId = arg.userId;
        }

        public async Task<IPagedResponse<Profile>> GetPage(int pageIndex)
        {
        if (pageIndex < 1)
            throw new ArgumentOutOfRangeException("pageIndex");


        GoodreadsAPI api = GoodreadsAPI.Instance;
        FriendSet friends = await api.GetFriends(userId, pageIndex);

        return new ProfileResponse(friends.Friends, friends.End, friends.Total);
        }
    }

    [DebuggerDisplay("PageIndex = {PageIndex} - VirtualCount = {VirtualCount}")]
    public class ProfileResponse : IPagedResponse<Profile>
    {
        public ProfileResponse(IEnumerable<Profile> items, int currentTotal, int virtualTotal)
        {
            this.Items = items;
            this.VirtualTotal = virtualTotal;
            this.CurrentTotal = currentTotal;
        }

        public int VirtualTotal { get; private set; }
        public int CurrentTotal { get; private set; }

        public IEnumerable<Profile> Items { get; private set; }
    }
}
