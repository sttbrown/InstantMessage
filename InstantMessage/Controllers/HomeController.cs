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
        /// <summary>
        /// Checks for existing user and creates and saves new user object if required. 
        /// Passes the messaging functionality to the client
        /// </summary>
        /// <returns>Html containing the messaging functionality</returns>
        [Authorize]
        public ActionResult Index()
        {
            //check if user already exists
            String current = System.Web.HttpContext.Current.GetOwinContext().Authentication.User.Identity.GetUserName();

            CurrentUser = _repo.getCurrentUser(current);

            if (CurrentUser == null)
            {
                _repo.createNewUser(current);

                return View("Main");
            }
            else
            {
                return View("Main");
            }
        }

        /// <summary>
        /// Passes the messaging functionality to the client
        /// </summary>
        /// <returns>Main.html</returns>
        [Authorize]
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.None,NoStore =true)]
        public ActionResult Main()
        {
            return View();
        }

        /// <summary>
        /// earlier prototype GUI
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult Conversation()
        {
            return View();
        }

        /// <summary>
        /// Test Code
        /// </summary>
        /// <returns></returns>
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
                return View();
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