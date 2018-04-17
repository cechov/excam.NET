using DeleBil.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace DeleBil.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private ApplicationDbContext _db;
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ApplicationDbContext Db
        {
            get
            {
                return _db ?? HttpContext.GetOwinContext().GetUserManager<ApplicationDbContext>();
            }
            private set
            {
                _db = value;
            }
        }

        public ExpandoObject Exo()
        {
            string currentUserId = User.Identity.GetUserId();
            dynamic MyModel = new ExpandoObject();

            var userCars = Db.Cars
                .Where(car => car.Owner.Id == currentUserId)
                .ToList();

            var userCarsForLease = Db.Leases
                .Where(lease => lease.Car.Owner.Id == currentUserId && lease.Status != LeaseStatus.Verified)
                .ToList();

            var userRentals = Db.Leases
                .Where(lease => lease.Leaser.Id == currentUserId)
                .OrderByDescending(lease => lease.Id)
                .Select(lease => new LeaseDto
                {
                    Id = lease.Id,
                    CarLicensePlate = lease.Car.LicensePlate,
                    CarName = lease.Car.Title,
                    CarOwner = lease.Car.Owner.UserName,
                    LeaserUserName = lease.Leaser.UserName,
                    Status = lease.Status
                })
                .ToList();

            var userCarLeases = Db.Leases
                .Where(lease => lease.Car.Owner.Id == currentUserId)
                .OrderByDescending(lease => lease.Id)
                .Select(lease => new LeaseDto
                {
                    Id = lease.Id,
                    CarLicensePlate = lease.Car.LicensePlate,
                    CarName = lease.Car.Title,
                    CarOwner = lease.Car.Owner.UserName,
                    LeaserUserName = lease.Leaser.UserName,
                    Status = lease.Status
                })
                .ToList();

            int countCars = 0;
            int countCarsForLease = 0;
            int countRentals = 0;
            int countCarLeases = 0;

            if (userCars.Count() > 0) countCars = 1;
            if (userCarsForLease.Count() > 0) countCarsForLease = 1;
            if (userRentals.Count() > 0) countRentals = 1;
            if (userCarLeases.Count() > 0) countCarLeases = 1;

            var cars = Db.Cars.Where(car => car.Owner.Id == currentUserId).ToList();

            MyModel.CountCars = countCars;
            MyModel.UserCars = userCars;
            MyModel.CountCarsForLease = countCarsForLease;
            MyModel.UserCarsForLease = userCarsForLease;
            MyModel.CountRentals = countRentals;
            MyModel.UserRentals = userRentals;
            MyModel.CountCarLeases = countCarLeases;
            MyModel.UserCarLeases = userCarLeases;
            return MyModel;
        }

        public ActionResult Index()
        {
            return View(Exo());
        }

        // GET: Cars/Create
        public ActionResult Newcar()
        {
            return View();
        }

        // POST: Cars/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewCar([Bind(Include = "Id,LicensePlate,Title")] Car car, HttpPostedFileBase file)
        {
            try
            {
                    car.Owner = UserManager.FindById(User.Identity.GetUserId());

                    Picture picture = new Picture();
                    if (file != null && file.ContentLength > 0)
                    {
                        picture.PictureData = SaveImageFile(file);
                        car.Picture = picture;
                    }

                    Db.Pictures.Add(picture);
                    Db.Cars.Add(car);
                    Db.SaveChanges();
            }
            catch (RetryLimitExceededException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View("Index", Exo());
        }

        private byte[] SaveImageFile(HttpPostedFileBase file)
        {
            byte[] pictureData;
            using (var memoryStream = new MemoryStream(file.ContentLength))
            {
                file.InputStream.CopyTo(memoryStream);
                pictureData = memoryStream.ToArray();
            }

            return pictureData;
        }

        public ActionResult DeleteCar(int? id)
        {
            try
            {
                Car car = Db.Cars.Find(id);
                if (car != null)
                {
                    Db.Cars.Remove(car);
                    Db.SaveChanges();
                }
                return RedirectToAction("Index", Exo());
            }
            catch
            {
                new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View();
        }

        public ActionResult DeleteLease(int? id)
        {
            try
            {
                Lease lease = Db.Leases.Find(id);
                if (lease != null)
                {
                    Db.Leases.Remove(lease);
                    Db.SaveChanges();
                }
                return RedirectToAction("Index", Exo());
            }
            catch
            {
                new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View();
        }
    }
}