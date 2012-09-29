using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel.Model
{
    public class UserStatus
    {
        public int Id { get; set; }
        public String Body { get; set; }
        public int BookId { get; set; }
        public int Page { get; set; }
        public int Percent { get; set; }
    }
}
