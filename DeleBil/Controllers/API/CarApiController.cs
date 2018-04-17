using DeleBil.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace DeleBil.Controllers.API
{
    public class CarApiController : ApiController
    {
        private ApplicationDbContext _db;
        public ApplicationDbContext Db
        {
            get
            {
                return _db ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationDbContext>();
            }
            private set
            {
                _db = value;
            }
        }
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [HttpPost]
        [Route("api/Cars")]
        [ResponseType(typeof(int))]
        public async Task<IHttpActionResult> PostCar()
        {
            var owner = await UserManager.FindByNameAsync(User.Identity.Name);

            var request = HttpContext.Current.Request;

            Picture picture = new Picture
            {
                PictureData = PictureToByte(request.Files[0])
            };

            var car = new Car
            {
                LicensePlate = request.Form["Licenseplate"],
                Owner = owner,
                Title = request.Form["Title"],
                Picture = picture
            };

            if (string.IsNullOrWhiteSpace(car.LicensePlate) || string.IsNullOrWhiteSpace(car.Title) || request.Files.Count == 0)
            {
                return BadRequest(ModelState);
            }

            Db.Pictures.Add(picture);
            Db.Cars.Add(car);
            Db.SaveChanges();
            return Ok(car.Id);
        }

        [HttpGet]
        [Route("api/Leases/{lat:double}/{lng:double}/")]
        public IHttpActionResult GetAvailableLeases(double lat, double lng)
        {
            var leases = Db.Leases
                .Where(lease => lease.Status == LeaseStatus.Available)
                .Where(lease => lease.PickupLocation.Location.Latitude < lat + 0.3 && lease.PickupLocation.Location.Latitude > lat - 0.3)
                .Where(lease => lease.PickupLocation.Location.Longitude < lng + 0.3 && lease.PickupLocation.Location.Longitude > lng - 0.3)
                .ToArray()
                .Select(lease => new LeaseDto
                {
                    CarLicensePlate = lease.Car.LicensePlate,
                    CarName = lease.Car.Title,
                    CarOwner = lease.Car.Owner.UserName,
                    CarPicture = lease.Car.Picture,
                    latitudePickUpLocation = (double)lease.PickupLocation.Location.Latitude,
                    longtitudePickUpLocation = (double)lease.PickupLocation.Location.Longitude,
                    PicUpLocationPictures = lease.PickupLocation.Pictures,
                    Status = lease.Status
                });

            return Ok(leases);
        }

        [HttpGet]
        [Route("api/Cars/{RegNr}")]
        public IHttpActionResult GetCar(string RegNr)
        {
            var car = Db.Cars
                .Where(c => c.LicensePlate == RegNr)
                .ToArray();

            if (car.Count() == 0)
            {
                return Ok(0);
            }
            
            return Ok(1);
        }

        [HttpGet]
        [Route("api/Leases/{RegNr}")]
        public IHttpActionResult GetLease(string RegNr)
        {
            var lease = Db.Leases
                .Where(l => l.Car.LicensePlate == RegNr)
                .Select(l => new LeaseDto
                {
                    CarLicensePlate = l.Car.LicensePlate,
                    CarName = l.Car.Title,
                    CarOwner = l.Car.Owner.UserName,
                    latitudePickUpLocation = (double)l.PickupLocation.Location.Latitude,
                    longtitudePickUpLocation = (double)l.PickupLocation.Location.Longitude,
                    PicUpLocationPictures = l.PickupLocation.Pictures,
                    CarPicture = l.Car.Picture,
                    Status = l.Status
                }).ToArray();

            if (lease.Count() == 0)
            {
                var emptyLease = new LeaseDto
                {
                    CarLicensePlate = RegNr,
                    Status = LeaseStatus.Verified
                };
                return Ok(emptyLease);
            }

            return Ok(lease.Last());
        }

        [HttpPost]
        [Route("api/Leases/NewLease")]
        [ResponseType(typeof(int))]
        public async Task<IHttpActionResult> NewLease(LeaseRequest request)
        {
            Car car = Db.Cars.Where(c => c.LicensePlate == request.RegNr).Single();

            foreach (Picture picture in request.Pictures)
            {
                Db.Pictures.Add(picture);
            }

            Parking parking = new Parking
            {
                Location = CreateLocation(request.Lat, request.Lng),
                Pictures = request.Pictures
            };

            Lease lease = new Lease
            {
                Car = car,
                Status = LeaseStatus.Available,
                PickupLocation = parking
            };

            Db.Parkings.Add(parking);
            Db.Leases.Add(lease);
            Db.SaveChanges();

            return Ok(car.Id);
        }

        [HttpPut]
        [Route("api/Leases/UpdateLease")]
        [ResponseType(typeof(int))]
        public async Task<IHttpActionResult> UpdateLease(LeaseRequest request)
        {
            //var leaser = await UserManager.FindByNameAsync(request.UserId);
            var leaser = await UserManager.FindByEmailAsync("anotheruser@gmail.com");
            Lease lease = Db.Leases.Where(l => l.Car.LicensePlate == request.RegNr).ToArray().Last();
            
            switch (lease.Status)
            {
                case LeaseStatus.Available:
                    lease.Status = LeaseStatus.Rented;
                    lease.Leaser = leaser;
                    break;
                case LeaseStatus.Rented:
                    foreach (Picture picture in request.Pictures)
                    {
                        Db.Pictures.Add(picture);
                    }
                    lease.Status = LeaseStatus.Delivered;
                    lease.DeliveryLocation = new Parking
                    {
                        Location = CreateLocation(request.Lat, request.Lng),
                        Pictures = request.Pictures
                    };
                    Db.Parkings.Add(lease.DeliveryLocation);
                    break;
                default:
                    break;
            }
            
            Db.Entry(lease).State = EntityState.Modified;
            Db.SaveChanges();

            return Ok(lease.Id);
        }

        private byte[] PictureToByte(HttpPostedFile file)
        {
            byte[] pictureData;
            using (var memoryStream = new MemoryStream(file.ContentLength))
            {
                file.InputStream.CopyTo(memoryStream);
                pictureData = memoryStream.ToArray();
            }

            return pictureData;
        }

        public static DbGeography CreateLocation(double latitude, double longitude)
        {
            var point = string.Format(CultureInfo.InvariantCulture.NumberFormat,
                                     "POINT({0} {1})", longitude, latitude);
            // 4326 is most common coordinate system used by GPS/Maps
            return DbGeography.PointFromText(point, 4326);
        }
    }

    public class LeaseRequest
    {
        [Required]
        public string RegNr { get; set; }
        [Required]
        public double Lat { get; set; }
        [Required]
        public double Lng { get; set; }
        [Required]
        public IList<Picture> Pictures { get; set; }
    }
}
