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

namespace DeleBil.Droid
{
    public class Picture
    {
        public int Id { get; set; }
        public byte[] PictureData { get; set; }
    }
}