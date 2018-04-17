using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DeleBil.Models
{
    public class CarDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vennligst fyll inn bilens navn.")]
        [Display(Name = "Navn")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vennligst fyll inn skiltnummer.")]
        [Display(Name = "Skiltnummer")]
        public string LicensePlate { get; set; }

        [Required]
        [Display(Name = "Eier")]
        public string OwnerUserName { get; set; }

        [Required(ErrorMessage = "Vennligst last opp bilde av bilen.")]
        [Display(Name = "Bilde")]
        [DataType(DataType.ImageUrl)]
        public string Picture { get; set; }

        public LeaseStatus Status { get; set; }
    }
}