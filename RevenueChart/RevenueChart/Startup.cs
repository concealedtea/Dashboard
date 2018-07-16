using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RevenueChart.Startup))]
namespace RevenueChart
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
