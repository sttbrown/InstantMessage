using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(InstantMessage.Startup))]
namespace InstantMessage
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            
        }

        //private void DeleteUser()
        //{
        //    string userName = "stewart.t.brown@gmail.com";
        //    System.Web.Security.Membership.DeleteUser(userName);

        //    System.Diagnostics.Debug.WriteLine("delete user method complete");

        //}
    }
}
