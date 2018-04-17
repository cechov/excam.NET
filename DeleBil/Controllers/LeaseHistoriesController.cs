using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DeleBil.Models;

namespace DeleBil.Controllers
{
    [Authorize]
    public class LeaseHistoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: LeaseHistories
        public ActionResult Index()
        {
            return View(db.LeaseHistories.ToList());
        }

        // GET: LeaseHistories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeaseHistory leaseHistory = db.LeaseHistories.Find(id);
            if (leaseHistory == null)
            {
                return HttpNotFound();
            }
            return View(leaseHistory);
        }

        // GET: LeaseHistories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LeaseHistories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,DateLeased,DateDelivered")] LeaseHistory leaseHistory)
        {
            if (ModelState.IsValid)
            {
                db.LeaseHistories.Add(leaseHistory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(leaseHistory);
        }

        // GET: LeaseHistories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeaseHistory leaseHistory = db.LeaseHistories.Find(id);
            if (leaseHistory == null)
            {
                return HttpNotFound();
            }
            return View(leaseHistory);
        }

        // POST: LeaseHistories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,DateLeased,DateDelivered")] LeaseHistory leaseHistory)
        {
            if (ModelState.IsValid)
            {
                db.Entry(leaseHistory).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(leaseHistory);
        }

        // GET: LeaseHistories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeaseHistory leaseHistory = db.LeaseHistories.Find(id);
            if (leaseHistory == null)
            {
                return HttpNotFound();
            }
            return View(leaseHistory);
        }

        // POST: LeaseHistories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            LeaseHistory leaseHistory = db.LeaseHistories.Find(id);
            db.LeaseHistories.Remove(leaseHistory);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
