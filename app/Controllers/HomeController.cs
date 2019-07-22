using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using app.Models;

namespace app.Controllers
{
    public class HomeController : Controller
    {
        public SageOAuthModel oAuthContent { get; set; } 
        
        public string UrlAuthorizeApiAccess { get; set; }

        public string AccessToken { get; set; }


        public HomeController()
        {
             // SageOAuthModel oAuthContent = new SageOAuthModel();
             // oAuthContent.callbackUrl = "https://www.sageone.com/oauth2/auth/central?filter=apiv3.1&response_type=code&client_id=" + Config.ClientId + "&redirect_uri=https://localhost:5001/auth/callback&scope=full_access&state=1234567";
             UrlAuthorizeApiAccess = Config.BaseUrl + "/login?authscheme=oauth2";
             AccessToken = "abcd";
        
        }
        public IActionResult Index()
        {   
            Console.WriteLine("<HomeController -> Index\n");
            HttpContext.Session.SetString("HomeController -> Index", "John");
            foreach(var k in HttpContext.Session.Keys){
                Console.WriteLine("*** " + k.ToString() + " -> " + HttpContext.Session.GetString(k));
                }
            Console.WriteLine(">\n");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Guide()
        {   
            HttpContext.Session.SetString("HomeController -> Guide", "event!!!!!!");
            // HttpContext.Session.SetString("accessTokenController", "abcdefg");
            // oAuthContent.testVal1 = HttpContext.Session.GetString("accessTokenEvent") ?? "event->null";
            // oAuthContent.testVal2 = HttpContext.Session.GetString("accessTokenController") ?? "event->null";
            
            // this.AddAuthButton = "<button href=\"https://www.sageone.com/oauth2/auth/central?filter=apiv3.1&response_type=code&client_id=" + Config.ClientId + "&redirect_uri=https://localhost:5001/auth/callback&scope=full_access&state=1234567\">Authorize API access</button>";
            // this.Message = "<code>test</code>";
            
            // oAuthContent.callbackUrl = "https://www.sageone.com/oauth2/auth/central?filter=apiv3.1&response_type=code&client_id=" + Config.ClientId + "&redirect_uri=https://localhost:5001/auth/callback&scope=full_access&state=1234567";
            // so.authCode = "abc";
            // Console.WriteLine("cb: " + oAuthContent.callbackUrl);
            // ViewData["UrlAuthorizeApiAccess"] = UrlAuthorizeApiAccess;

            foreach(var k in HttpContext.Session.Keys){
                Console.WriteLine("*** " + k.ToString() + " -> " + HttpContext.Session.GetString(k));
                }
            Console.WriteLine(">\n");

            // SageOAuthModel oAuthContent = new SageOAuthModel();
            // oAuthContent.testVal1 = HttpContext.Session.GetString("Name");

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
