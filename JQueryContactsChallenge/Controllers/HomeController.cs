using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace JQueryContactsChallenge.Controllers
{
    public class HomeController : Controller
    {
        private Models.JQueryContactsChallengeEntities _dbEntities = new Models.JQueryContactsChallengeEntities();

        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult About()
        {
            this.ViewBag.Message = "Your application description page.";

            return this.View();
        }

        public ActionResult Contact()
        {
            this.ViewBag.Message = "Your contact page.";

            return this.View();
        }

        public ActionResult Create() { return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Phone,Email")]Models.Contact contact)
        {
            if (ModelState.IsValid)
            {
                contact.Id = Guid.NewGuid();
                this._dbEntities.Contacts.Add(contact);
                this._dbEntities.SaveChanges();
                return this.RedirectToAction("Index");
            }

            return this.View(contact);
        }

        public ActionResult Edit(Guid? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Models.Contact contact = this._dbEntities.Contacts.Find(id);
            if (contact == null)
                return this.HttpNotFound();

            return this.View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Phone,Email")] Models.Contact contact)
        {
            if (ModelState.IsValid)
            {
                this._dbEntities.Entry(contact).State = EntityState.Modified;
                this._dbEntities.SaveChanges();
                return this.RedirectToAction("Index");
            }

            return this.View(contact);
        }

        [HttpPost]
        public ActionResult Delete(Guid? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Models.Contact contact = this._dbEntities.Contacts.Find(id.Value);
            if (contact == null)
                return new JsonResult { Data = new { success = false, message = String.Format("Item with Id of {0} not found.", id.Value.ToString()) } };

            try
            {
                this._dbEntities.Contacts.Remove(contact);
                this._dbEntities.SaveChanges();
            }
            catch (Exception exc)
            {
                return new JsonResult { Data = new { success = false, message = String.Format("Error deleting item with Id of {0}: {1}", id.Value.ToString(), exc.ToString()) } };
            }

            return new JsonResult { Data = new { success = true, message = String.Format("Item Id {0} deleted.", id.Value.ToString()) } };
        }

        public ActionResult Details(Guid? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Models.Contact contact;
            try
            {
                contact = this._dbEntities.Contacts.Find(id.Value);
            }
            catch (Exception exc)
            {
                return new JsonResult { Data = new { id = id, message = String.Format("Error deleting item with Id of {0}: {1}", id.Value.ToString(), exc.ToString()) } };
            }

            if (contact == null)
                return new JsonResult { Data = new { id = id, message = String.Format("Item with Id of {0} not found.", id.Value.ToString()) } };

            return new JsonResult { Data = new { result = contact, id = id.Value } };
        }

        public ActionResult Search(string startsWith, long? concurrencyId)
        {
            if (!concurrencyId.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            IEnumerable<Models.Contact> contacts = this._dbEntities.Contacts;
            if (!String.IsNullOrWhiteSpace(startsWith)) {
                string s = startsWith.Trim();
                contacts = contacts.Select(c => new { N = (c.Name == null) ? "" : c.Name.Trim(), C = c })
                    .Where(a => a.N.Length >= s.Length && String.Compare(a.N.Substring(0, s.Length), s, true) == 0)
                    .Select(a => a.C);
            }

            return new JsonResult
            {
                Data = new
                {
                    concurrencyId = concurrencyId.Value,
                    result = contacts.Select(c => new
                    {
                        Contact = c,
                        EditAction = Url.Action("Edit", "Home", new { id = c.Id })
                    })
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this._dbEntities.Dispose();

            base.Dispose(disposing);
        }
    }
}