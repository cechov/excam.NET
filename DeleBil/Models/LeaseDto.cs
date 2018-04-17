using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;

namespace DeleBil.Models
{
    public class LeaseDto
    {
        public int Id { get; set; }

        [Display(Name = "Navn")]
        public string CarName { get; set; }

        [Display(Name = "Skiltnummer")]
        public string CarLicensePlate { get; set; }
        
        public Picture CarPicture { get; set; }

        [Display(Name = "Eier")]
        public string CarOwner { get; set; }

        [Display(Name = "Hentested")]
        public double longtitudePickUpLocation { get; set; }

        [Display(Name = "Hentested")]
        public double latitudePickUpLocation { get; set; }

        [Display(Name = "Leveringssted")]
        public double longtitudeDeliveryLocation { get; set; }

        [Display(Name = "Leveringssted")]
        public double latitudeDeliveryLocation { get; set; }

        [Display(Name = "Leietaker")]
        public string LeaserUserName { get; set; }

        public LeaseStatus Status { get; set; }

        [DataType(DataType.ImageUrl)]
        public IList<Picture> DeliveryLocationPictures { get; set; }

        [DataType(DataType.ImageUrl)]
        public IList<Picture> PicUpLocationPictures { get; set; }
    }
}