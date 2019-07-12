using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace app.Controllers
{
    [Route("auth/callback/")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        // GET auth/callback
        [HttpGet]
        public void GetData(string code, string country, string state)
        {
            Console.WriteLine("*** Callback ***");
            Console.WriteLine("Authorization Code: " + code);
            Console.WriteLine("Country: " + country);
            Console.WriteLine("State: " + state);

            Task<KeyValuePair<string, string>> qryAccessToken = HttpQueries.getAccessTokenByAuthCode(code);

            qryAccessToken.Wait();

            if (qryAccessToken.Result.Key == "accessToken")
            {
                Console.WriteLine("\nGot AccessToken, query sample endpoint\n");
                HttpQueries.queryCountries(qryAccessToken.Result.Value, "https://api.accounting.sage.com/v3.1/countries");
            }
            else
            {
                Console.WriteLine("\n" + qryAccessToken.Result.Key + ": " + qryAccessToken.Result.Value);
                Console.WriteLine("\nGot no AccessToken from authorization server\n");
            }

        }
    }
}
