using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TSAC.Bravo.PhotoContest.Web.Data;

[assembly: HostingStartup(typeof(TSAC.Bravo.PhotoContest.Web.Areas.Identity.IdentityHostingStartup))]
namespace TSAC.Bravo.PhotoContest.Web.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}