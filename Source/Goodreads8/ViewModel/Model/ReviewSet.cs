using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel.Model
{
    class ReviewSet
    {
        public int Start { get; set; }
        public int End { get; set; }
        public int Total { get; set; }
        public int CurrentPage { get; set; }
        public List<Review> Reviews { get; set; }
    }
}
