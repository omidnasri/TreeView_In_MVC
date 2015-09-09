using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TreeViewInMVC.Startup))]
namespace TreeViewInMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
