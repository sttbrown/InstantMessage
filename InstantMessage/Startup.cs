using Microsoft.AspNet.SignalR;
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
            app.MapSignalR();
            GlobalHost.HubPipeline.RequireAuthentication();
            //ensures no hub methods are accessible to users without authentication
        }

     
    }
}
