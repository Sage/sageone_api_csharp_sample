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

        public string UrlAuthorizeApiAccess { get; set; }

        public string AccessToken { get; set; }


        public HomeController()
        {
            // SageOAuthModel oAuthContent = new SageOAuthModel();
            // oAuthContent.callbackUrl = "https://www.sageone.com/oauth2/auth/central?filter=apiv3.1&response_type=code&client_id=" + Config.ClientId + "&redirect_uri=https://localhost:5001/auth/callback&scope=full_access&state=1234567";
            UrlAuthorizeApiAccess = Config.BaseUrl + "/login";
        }

        [HttpGet]
        public IActionResult ApiRequest(String http_verb, String resource)
        {

            Console.WriteLine("POST AT MyAction - " + http_verb + " - " + resource);
            
            HttpClient client = new HttpClient();
            HttpRequestMessage request;

            switch (http_verb)
            {
                case "get":
                    request = new HttpRequestMessage(HttpMethod.Get, Config.ApiBaseEndpoint + resource);
                    break;
                case "post":
                    request = new HttpRequestMessage(HttpMethod.Post, Config.ApiBaseEndpoint + resource);
                    break;
                case "put":
                    request = new HttpRequestMessage(HttpMethod.Put, Config.ApiBaseEndpoint + resource);
                    break;
                case "delete":
                    request = new HttpRequestMessage(HttpMethod.Delete, Config.ApiBaseEndpoint + resource);
                    break;
                default:
                    request = new HttpRequestMessage(HttpMethod.Get, Config.ApiBaseEndpoint + resource);
                    break;
            }
            
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("access_token") ?? ""); // Bearer
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Task<HttpResponseMessage> responseMsgAsync = client.SendAsync(request);
            Task.WaitAll(responseMsgAsync);

            HttpResponseMessage responseMsg = responseMsgAsync.Result;

            String responseStatusCode = responseMsg.StatusCode.ToString();
            String responseContent = responseMsg.Content.ReadAsStringAsync().Result;

            dynamic parsedJson = JsonConvert.DeserializeObject(responseContent);
            String responseContentPretty =  JsonConvert.SerializeObject(parsedJson, Formatting.Indented);

            HttpContext.Session.SetString("responseContent",responseContentPretty);
            HttpContext.Session.SetString("responseStatusCode", responseStatusCode);

            foreach (var k in HttpContext.Session.Keys)
            {
                Console.WriteLine("*** " + k.ToString() + " -> " + HttpContext.Session.GetString(k));
            }
            Console.WriteLine(">\n");
            // return Redirect(Config.BaseUrl + "/home/resp");
            //return View(model);
            return RedirectToAction("Index");

        }
        public IActionResult Index()
        {

            model.partialAccessTokenAvailable = "1";
            model.partialResposeIsAvailable = "1";
            /* Console.WriteLine("<HomeController -> Index\n");
            HttpContext.Session.SetString("HomeController -> Index", "John");
            foreach (var k in HttpContext.Session.Keys)
            {
                Console.WriteLine("*** " + k.ToString() + " -> " + HttpContext.Session.GetString(k));
            }
            Console.WriteLine(">\n");

            String session_access_token = HttpContext.Session.GetString("access_token") ?? "";
            String session_api_response_json = HttpContext.Session.GetString("api_response_json") ?? "";

            if (session_access_token.Length > 0 && session_api_response_json.Length > 0)
            {
                Console.WriteLine("redirect -> resp");
                return Redirect(Config.BaseUrl + "/home/resp");
            }
            else if (session_access_token.Length > 0 && session_api_response_json.Length == 0)
            {
                Console.WriteLine("redirect -> req");
                return Redirect(Config.BaseUrl + "/home/req");
            }
            else
            {
                Console.WriteLine("redirect -> guide");
                return Redirect(Config.BaseUrl + "/home/guide");
            } */

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
