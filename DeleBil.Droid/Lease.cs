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
    public class Lease
    {
        public int Id { get; set; }
        
        public string CarName { get; set; }
        
        public string CarLicensePlate { get; set; }
        
        public virtual Picture CarPicture { get; set; }
        
        public string CarOwner { get; set; }
        
        public double longtitudePickUpLocation { get; set; }
        
        public double latitudePickUpLocation { get; set; }
        
        public double longtitudeDeliveryLocation { get; set; }
        
        public double latitudeDeliveryLocation { get; set; }
        
        public string LeaserUserName { get; set; }

        public LeaseStatus Status { get; set; }
        
        public IList<Picture> DeliveryLocationPictures { get; set; }
        
        public IList<Picture> PicUpLocationPictures { get; set; }
    }

    public enum LeaseStatus
    {
        Available,
        Rented,
        Delivered,
        Verified
    }
}