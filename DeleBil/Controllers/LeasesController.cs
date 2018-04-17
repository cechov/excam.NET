using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DeleBil.Models;
using Microsoft.AspNet.Identity.Owin;

namespace DeleBil.Controllers
{
    [Authorize]
    public class LeasesController : Controller
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

        public LeaseDto GenerateLeaseDto(Lease leaseItem)
        {
            LeaseDto lease = new LeaseDto();
            lease.Id = leaseItem.Id;
            lease.CarLicensePlate = leaseItem.Car.LicensePlate;
            lease.CarName = leaseItem.Car.Title;
            lease.CarOwner = leaseItem.Car.Owner.UserName;
            lease.CarPicture = leaseItem.Car.Picture;
            lease.Status = leaseItem.Status;
            lease.latitudePickUpLocation = (double)leaseItem.PickupLocation.Location.Latitude;
            lease.longtitudePickUpLocation = (double)leaseItem.PickupLocation.Location.Longitude;
            lease.PicUpLocationPictures = leaseItem.PickupLocation.Pictures;

            switch (lease.Status)
            {
                case LeaseStatus.Rented:
                    lease.LeaserUserName = leaseItem.Leaser.UserName;
                    break;
                case LeaseStatus.Delivered:
                    lease.LeaserUserName = leaseItem.Leaser.UserName;
                    lease.latitudeDeliveryLocation = (double)leaseItem.DeliveryLocation.Location.Latitude;
                    lease.longtitudeDeliveryLocation = (double)leaseItem.DeliveryLocation.Location.Longitude;
                    lease.DeliveryLocationPictures = leaseItem.DeliveryLocation.Pictures;
                    break;
                default:
                    break;
            }

            return lease;
        }

        // GET: Leases/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var leaseItem = Db.Leases.Where(l => l.Id == id).Single();

            if (leaseItem == null)
            {
                return HttpNotFound();
            }

            return View(GenerateLeaseDto(leaseItem));
        }

        public ActionResult ChangeStatus(int? id)
        {
            try
            {
                Lease lease = Db.Leases.Find(id);
                if (lease != null)
                {
                    if (lease.Status == LeaseStatus.Delivered) lease.Status = LeaseStatus.Verified;
                    Db.Entry(lease).State = EntityState.Modified;
                    Db.SaveChanges();
                }
                return View("Details", GenerateLeaseDto(lease));
            }
            catch
            {
                new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View();
        }
        

        // GET: Leases/Delete/5
        public ActionResult DeleteLease(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lease lease = Db.Leases.Find(id);
            if (lease == null)
            {
                return HttpNotFound();
            }
            return View(lease);
        }

        // POST: Leases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Lease lease = Db.Leases.Find(id);
            Db.Leases.Remove(lease);
            Db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
