using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MovieTicketMVC.Startup))]
namespace MovieTicketMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            ConfigureAuth(app);
        }
    }
}