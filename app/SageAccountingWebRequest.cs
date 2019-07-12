using System;

using System.Net.Http;
using System.Net.Http.Headers;
namespace app
{
    public class SageAccountingWebRequest
    {
        public static async void queryAccountingApi(SageAccountingOAuth sageAuth, string endpointUrl)
        {

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(sageAuth.getAccessToken());


                Console.WriteLine("\nAsk api endpoint...");
                using (HttpResponseMessage response = await client.GetAsync(endpointUrl))
                {
                    using (HttpContent content = response.Content)
                    {
                        string result = await content.ReadAsStringAsync();
                        Console.WriteLine("---\nStatusCode: " + response.StatusCode);
                        Console.WriteLine("---\nHeaders: " + response.Headers);

                        if (result != null &&
                            result.Length >= 50)
                        {
                            // print the whole json response body
                            Console.WriteLine("---\nBody: \n" + result.ToString());

                        }
                    }
                }
            }
        }
    }

}
