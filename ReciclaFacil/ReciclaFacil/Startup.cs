using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ReciclaFacil.Startup))]
namespace ReciclaFacil
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
