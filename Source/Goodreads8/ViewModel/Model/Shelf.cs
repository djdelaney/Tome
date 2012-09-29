using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace Goodreads8.ViewModel.Model
{
    public class Shelf
    {
        public int Count { get; set; }
        public String Description { get; set; }
        public bool Exclusive { get; set; }
        public int Id { get; set; }
        public String Name { get; set; }


        public Brush BG { get; set; }
    }
}
