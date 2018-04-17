using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;

namespace DeleBil.Models
{
    public class Car
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vennligst fyll inn skiltnummer.")]
        [Display(Name = "Skiltnummer")]
        public string LicensePlate { get; set; }

        [Required]
        [Display(Name = "Eier")]
        public virtual ApplicationUser Owner { get; set; }

        [Required(ErrorMessage = "Vennligst last opp bilde av bilen.")]
        [Display(Name = "Bilde")]
        public virtual Picture Picture { get; set; }

        [Required(ErrorMessage = "Vennligst fyll inn bilens navn.")]
        [Display(Name = "Navn")]
        public string Title { get; set; }
    }

    public class Parking
    {
        public int Id { get; set; }
        public DbGeography Location { get; set; }
        public virtual IList<Picture> Pictures { get; set; }
    }

    public class Picture
    {
        public int Id { get; set; }
        public byte[] PictureData { get; set; }
    }
    
    public class Lease
    {
        public virtual Car Car { get; set; }
        public virtual Parking DeliveryLocation { get; set; }
        public int Id { get; set; }
        public virtual ApplicationUser Leaser { get; set; }
        public virtual Parking PickupLocation { get; set; }
        public LeaseStatus Status { get; set; }
    }

    public enum LeaseStatus
    {
        Available,
        Rented,
        Delivered,
        Verified
    }
}