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
    public class HomeController : Controller
    {
        
       // private InstantMessageContext Data = new InstantMessageContext();

        private DataRepository _repo = new DataRepository();

        private string AuthenticatedUser = System.Web.HttpContext.Current.GetOwinContext()
            .Authentication.User.Identity.GetUserId();

        private User CurrentUser;

        public HomeController()
        {

        }

        public ActionResult Index()
        {


            return View();
        }


        [Authorize]
        public ActionResult TestHome()
        {
            //check if user already exists
            String current = System.Web.HttpContext.Current.GetOwinContext().Authentication.User.Identity.GetUserId();

            CurrentUser = _repo.getCurrentUser(current);

            if (CurrentUser == null)
            {
                _repo.createNewUser(current);

                return View("NewUser");
            }
            else
            {
                //return users conversations. 

                List<Conversation> conversations = _repo.getAllConversations(CurrentUser);

                if (conversations == null)
                {
                    Console.WriteLine("no conversations available");
                    return View("NewUser");
                }

                return View(conversations);
            }
        }

        

        [Authorize]
        public ActionResult CreateConversation()
        {
            //All Users ,  way of selecting user - should this bring up modal or new page Post Datas?
            //Have a selected user , Create new conversation, add user into many-to-many table.
            //messages all allocated conversation ID 

           List<User> contacts= _repo.getAllContacts(AuthenticatedUser);

            return View(contacts);
        }


        [Authorize]
        public ActionResult Conversation(string id)
        {
            //this needs to be a POST
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
               
            }

            //start conversation with one other user

            User other = _repo.getContact(id);
            
            if (other == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            _repo.startConversation(CurrentUser, other);
 
            return View();
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