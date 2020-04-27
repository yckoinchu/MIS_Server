using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MIS_Server.Startup))]
namespace MIS_Server
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
