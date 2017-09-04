using InstantMessage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using InstantMessage.DAL;
using System.Net;

namespace InstantMessage
{
    //prototype class not in use
    public class RealTimeStart
    {
        private User current;
        private DataRepository _data = new DataRepository();

        public RealTimeStart(User current)
        {
            this.current = current;

        }




    }
}