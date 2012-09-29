using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.Common
{
    public interface IPagedResponse<T>
        {
        IEnumerable<T> Items { get; }
        int VirtualTotal { get; }
        int CurrentTotal { get; }
        }
}
