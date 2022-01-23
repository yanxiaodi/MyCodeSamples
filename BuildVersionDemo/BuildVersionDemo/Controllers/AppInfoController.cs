using BuildVersionDemo.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace BuildVersionDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppInfoController : ControllerBase
    {
        [HttpGet(Name = "GetAppInfo")]
        public AppInfo Get()
        {
            return new AppInfo()
            {
                Version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0"
            };
        }
    }
}
