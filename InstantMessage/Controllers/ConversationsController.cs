using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using InstantMessage.DAL;
using InstantMessage.Models;

namespace InstantMessage.Controllers
{
    /// <summary>
    /// Conversation Controller - used in development, not in final prototype.
    /// </summary>
    public class ConversationsController : Controller
    {
        private InstantMessageContext db = new InstantMessageContext();

        // GET: Conversations
        public ActionResult Index()
        {
            return View(db.Conversations.ToList());
        }

        // GET: Conversations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Conversation conversation = db.Conversations.Find(id);
            if (conversation == null)
            {
                return HttpNotFound();
            }
            return View(conversation);
        }

        // GET: Conversations/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Conversations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ConversationID,Name")] Conversation conversation)
        {
            if (ModelState.IsValid)
            {
                db.Conversations.Add(conversation);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(conversation);
        }

        // GET: Conversations/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Conversation conversation = db.Conversations.Find(id);
            if (conversation == null)
            {
                return HttpNotFound();
            }
            return View(conversation);
        }

        // POST: Conversations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ConversationID,Name")] Conversation conversation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(conversation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(conversation);
        }

        // GET: Conversations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Conversation conversation = db.Conversations.Find(id);
            if (conversation == null)
            {
                return HttpNotFound();
            }
            return View(conversation);
        }

        // POST: Conversations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Conversation conversation = db.Conversations.Find(id);
            db.Conversations.Remove(conversation);
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
