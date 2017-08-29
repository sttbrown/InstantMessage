using InstantMessage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using InstantMessage.DAL;
using System.Net;

namespace InstantMessage.Controllers
{   
    [RequireHttps]
    [OutputCache(Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
    public class HomeController : Controller
    {
        private DataRepository _repo = new DataRepository();

        private string AuthenticatedUser = System.Web.HttpContext.Current.GetOwinContext()
            .Authentication.User.Identity.GetUserName();

        private User CurrentUser;

        public HomeController()
        {

        }

        [Authorize]
        public ActionResult Index()
        {
            //check if user already exists
            String current = System.Web.HttpContext.Current.GetOwinContext().Authentication.User.Identity.GetUserName();

            CurrentUser = _repo.getCurrentUser(current);

            if (CurrentUser == null)
            {
                _repo.createNewUser(current);

                return View("Conversation");
            }
            else
            {
                //return users conversations. 

                //List<Conversation> conversations = _repo.GetAllConversations(CurrentUser);

                //if (conversations == null)
                //{
                //    Console.WriteLine("no conversations available");
                //    return View("NewUser");
                //}

                return View("Conversation");
            }
        }

        [Authorize]
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.None,NoStore =true)]
        public ActionResult Main()
        {
            return View();
        }


        [Authorize]
        public ActionResult Conversation()
        {
            return View();
        }


        [Authorize]
        public ActionResult TestHome()
        {
            //check if user already exists
            String current = System.Web.HttpContext.Current.GetOwinContext().Authentication.User.Identity.GetUserName();

            CurrentUser = _repo.getCurrentUser(current);

            if (CurrentUser == null)
            {
                _repo.createNewUser(current);

                return View("NewUser");
            }
            else
            {
                //return users conversations. 

                //List<Conversation> conversations = _repo.GetAllConversations(CurrentUser);

                //if (conversations == null)
                //{
                //    Console.WriteLine("no conversations available");
                //    return View("NewUser");
                //}

                return View();
            }
        }

        

        [Authorize]
        public ActionResult CreateConversation()
        {
            //All Users ,  way of selecting user - should this bring up modal or new page Post Datas?
            //Have a selected user , Create new conversation, add user into many-to-many table.
            //messages all allocated conversation ID 

           List<User> contacts= _repo.GetAllContacts(AuthenticatedUser);

            return View(contacts);
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