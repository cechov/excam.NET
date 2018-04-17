using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DeleBil.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace DeleBil.Controllers
{
    [Authorize]
    public class CarsController : Controller
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

        // GET: Cars/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var carItem = Db.Cars.Where(car => car.Id == id).Single();

            var CarDto = new CarDto
            {
                Id = carItem.Id,
                LicensePlate = carItem.LicensePlate,
                OwnerUserName = carItem.Owner.UserName,
                Picture = "data:image/jpeg;base64, " + Convert.ToBase64String(carItem.Picture.PictureData),
                Title = carItem.Title,

            };

            var leaseItem = Db.Leases.Where(lease => lease.Car.Id == carItem.Id).ToArray();

            if (leaseItem.Count() == 0) CarDto.Status = LeaseStatus.Verified;
            else
            {
                var lease = leaseItem.Last();
                CarDto.Status = lease.Status;
            }

            return View(CarDto);
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

        // GET: Cars/Create
        public ActionResult Newcar()
        {
            return View();
        }

        // GET: Cars/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var car = Db.Cars.Where(c => c.Id == id).Single();
            
            if (car == null)
            {
                return HttpNotFound();
            }
            return View(car);
        }

        // POST: Cars/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,LicensePlate,Title")] Car carEdited, HttpPostedFileBase file)
        {
            try
            {
                Car car = Db.Cars.Where(c => c.Id == carEdited.Id).Single();
                Picture picture = Db.Pictures.Where(p => p.Id == car.Picture.Id).Single();

                if (file != null && file.ContentLength > 0)
                {
                    var oldPicture = picture.PictureData;
                    var newPicture = SaveImageFile(file);
                    if (!newPicture.Equals(oldPicture)) picture.PictureData = newPicture;

                    Db.Entry(picture).State = EntityState.Modified;
                    car.Picture = picture;
                }
                else car.Picture = picture;

                car.Owner = UserManager.FindById(User.Identity.GetUserId());
                car.LicensePlate = carEdited.LicensePlate;
                car.Title = carEdited.Title;

                Db.Entry(car).State = EntityState.Modified;
                Db.SaveChanges();
                
                var CarDto = new CarDto
                {
                    Id = car.Id,
                    LicensePlate = car.LicensePlate,
                    OwnerUserName = car.Owner.UserName,
                    Picture = "data:image/jpeg;base64, " + Convert.ToBase64String(car.Picture.PictureData),
                    Title = car.Title
                };

                var leaseItem = Db.Leases.Where(l => l.Car.Id == car.Id).ToArray();

                if (leaseItem.Count() == 0) CarDto.Status = LeaseStatus.Verified;
                else
                {
                    var lease = leaseItem.Last();
                    CarDto.Status = lease.Status;
                }

                return View("Details", CarDto);
            }
            catch (RetryLimitExceededException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            
            return View();
        }
    }
}
