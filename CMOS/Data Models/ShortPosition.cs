using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace CMOS.Data_Models
{
    public class ShortPosition
    {
        public int Id { get; set; }
        public double Weight { get; set; }
        public int Rate { get; set; }
    }
}