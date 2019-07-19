using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
// using app.Models;

namespace app.Controllers
{
    public class HomeController : Controller
    {
        // SageOAuthModel oAuthContent = new SageOAuthModel();
        public string Message { get; set; }
        public string AddAuthButton { get; set; } 

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Guide()
        {
            // this.AddAuthButton = "<button href=\"https://www.sageone.com/oauth2/auth/central?filter=apiv3.1&response_type=code&client_id=" + Config.ClientId + "&redirect_uri=https://localhost:5001/auth/callback&scope=full_access&state=1234567\">Authorize API access</button>";
            this.Message = "<code>test</code>";
            // SageOAuthModel so = new SageOAuthModel();
            //so.authCode = "abc";

            return View();
        }

        public IActionResult Req()
        {
            return View();
        }

        public IActionResult Resp()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            return View();
        }
    }
}
