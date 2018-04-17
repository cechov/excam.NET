using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DeleBil.Startup))]
namespace DeleBil
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
