using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using app.Models;

namespace app.Controllers
{
    public class HomeController : Controller
    {
        ContentModel model = new ContentModel();


        public IActionResult Index()
        {

            // default values
            var sessionName = new Byte[20];
            if (!HttpContext.Session.TryGetValue("reqEndpoint",out sessionName))
                HttpContext.Session.SetString("reqEndpoint", "user");


            if (!HttpContext.Session.TryGetValue("access_token",out sessionName))
            {
              // access_token field is empty
              model.partialAccessTokenAvailable = "0";
              model.partialResposeIsAvailable = "0";
            }
            else if (HttpContext.Session.TryGetValue("access_token",out sessionName) && !HttpContext.Session.TryGetValue("responseContent",out sessionName)) 
            {
              // access_token filled and responseContent is empty
              model.partialAccessTokenAvailable = "1";
              model.partialResposeIsAvailable = "0";
            }
            else 
            {
              model.partialAccessTokenAvailable = "1";
              model.partialResposeIsAvailable = "1";
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Guide()
        {
            HttpContext.Session.SetString("BaseUrl", Config.BaseUrl);
            HttpContext.Session.SetString("HomeController -> Guide", "event!!!!!!");

            foreach (var k in HttpContext.Session.Keys)
            {
                Console.WriteLine("*** " + k.ToString() + " -> " + HttpContext.Session.GetString(k));
            }
            Console.WriteLine(">\n");

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
