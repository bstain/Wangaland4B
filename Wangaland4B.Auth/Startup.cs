using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Wangaland4B.Auth.Startup))]
namespace Wangaland4B.Auth
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
