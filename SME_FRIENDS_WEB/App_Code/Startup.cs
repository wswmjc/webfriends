using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;

[assembly: OwinStartup(typeof(Startup))]

public class Startup
{
    public void Configuration(IAppBuilder app)
    {

        GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => new CookiesUserIdProvider());
        app.MapSignalR("/share", new HubConfiguration());
    }
}
