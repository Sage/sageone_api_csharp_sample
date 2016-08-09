using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Sage_One_API_Sample_Website.Startup))]
namespace Sage_One_API_Sample_Website
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
