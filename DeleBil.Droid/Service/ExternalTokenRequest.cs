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

namespace DeleBil.Droid.Service
{
    public class ExternalTokenRequest
    {
        public string Token { get; set; }

        public string Provider { get; set; }
    }
}