using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel.Model
{
    class Event
    {
        public int Id { get; set; }
        public String Title { get; set; }
        public String Venue { get; set; }
        public String Link { get; set; }
        public String Description { get; set; }
        public String ImageUrl { get; set; }
        public String City { get; set; }
        public String StateCode { get; set; }
        public String CountryCode { get; set; }
        public String EventType { get; set; }
        public DateTime StartAt { get; set; }
    }
}
