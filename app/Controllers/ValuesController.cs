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
            SageAccountingOAuth sageAuth = new SageAccountingOAuth();

            Console.WriteLine("*** Callback ***");
            Console.WriteLine("Authorization Code: " + code);
            Console.WriteLine("Country: " + country);
            Console.WriteLine("State: " + state);

            Task t = sageAuth.requestAccessTokenByAuthCode(code);
            t.Wait();
         
/*             for(int i = 305; i>0; i--)
            {   
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                Console.Write(".." + i);
                if(i % 20 == 0) {
                    Console.Write("\n");
                }
            }

            t = sageAuth.requestNewAccessTokenByRefreshToken();
            t.Wait(); */

            if (sageAuth.getAuthState().Equals("valid"))
            {
                SageAccountingWebRequest.queryAccountingApi(sageAuth, "https://api.accounting.sage.com/v3.1/countries");
            }
            else
            {
                Console.WriteLine("\n" + sageAuth.getAuthState());
                Console.WriteLine("\nGot no AccessToken from authorization server\n");
            }

        }
    }
}
