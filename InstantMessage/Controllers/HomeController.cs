using InstantMessage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using InstantMessage.DAL;

namespace InstantMessage.Controllers
{   
    [RequireHttps]
    public class HomeController : Controller
    {

        private InstantMessageContext Data = new InstantMessageContext();

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult CreateConversation()
        {
            //All Users ,  way of selecting user - should this bring up modal or new page Post Datas?

            //Have a selected user , Create new conversation, add user into many-to-many table.

            //messages all allocated conversation ID 

            List<User> all = Data.Users.ToList();
            List<User> contacts = new List<User>();

            foreach (User u in all)
            {
                if (u.UserID != System.Web.HttpContext.Current.GetOwinContext().
                    Authentication.User.Identity.GetUserId())
                {
                    contacts.Add(u);
                }
            }

            return View(contacts);
        }


        [Authorize]
        public ActionResult TestHome()
        {
            //check if user already exists
            String current = System.Web.HttpContext.Current.GetOwinContext().Authentication.User.Identity.GetUserId();

            System.Console.WriteLine("Hello, "+ current );

            User user = Data.Users.Find(current);

            if (user == null)
            {
                user = new User(current);
                Data.Users.Add(user);
                Data.SaveChanges(); 
                return View("NewUser");
            }
            else
            {
                //List<Conversation> Conversations = data.Conversations.Where<>
                // Where(l => l.Courses.Select(c => c.CourseId).Contains(courseId)

                List<Conversation> conver = new List<Conversation>();

                try
                {
                    conver = Data.Conversations.ToList();
                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine("no conversations available");
                    return View("NewUser");
                }

                List<Conversation> userCon = new List<Conversation>();

                    foreach (Conversation c in conver)
                    {
                        if (c.Users.Contains(user))
                        {
                            userCon.Add(c);
                        }
                    }

                return View(userCon);
            }

            
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}