using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Croppic.Startup))]
namespace Croppic
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
