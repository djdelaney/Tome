using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel.Model
{
    class Comparison
    {
        public Book Book { get; set; }
        public Review MyReview { get; set; }
        public Review TheirReview { get; set; }
    }
}
