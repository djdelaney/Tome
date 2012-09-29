using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.Common
{
    public interface IPagedSource<T>
        {
        void SetArguments(Object argument);

        Task<IPagedResponse<T>> GetPage(int pageIndex);
        }
}
