using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
namespace app
{
    public class SageAccountingOAuth
    {
        private const string AUTHORIZE_URL = "https://www.sageone.com/oauth2/auth/central?filter=apiv3.1";        // Authorisation URL

        private const string ACCESS_TOKEN_URL = "https://oauth.accounting.sage.com/token";       // Access Token URL
        private string callbackUrl = Config.CallbackUrl;       // Call back URL - this should match the Callback URL reigstered against your application on https://developers.sageone.com/
        private string clientID = Config.ClientId;             // Client ID - this should match the Client ID reigstered against your application on https://developers.sageone.com/
        private string clientSecret = Config.ClientSecret;     // Client Secret - this should match the Client Secret reigstered against your application on https://developers.sageone.com/
        private string authCode = "";
        private string accessToken = "";
        private string refreshToken = "";
        private string authState = "";

        public SageAccountingOAuth()
        {
        }

        public async Task<int> requestAccessTokenByAuthCode(String authCode)
        {
            this.authCode = authCode;

            using (HttpClient client = new HttpClient())
            {
                var dict = new Dictionary<string, string>();
                dict.Add("client_id", this.clientID);
                dict.Add("client_secret", this.clientSecret);
                dict.Add("code", this.authCode);
                dict.Add("grant_type", "authorization_code");
                dict.Add("redirect_uri", this.callbackUrl);

                var request = new HttpRequestMessage(HttpMethod.Post, ACCESS_TOKEN_URL)
                {
                    Content = new FormUrlEncodedContent(dict)
                };

                Console.WriteLine("\nAsk for AccessToken...");
                using (HttpResponseMessage response = await client.SendAsync(request))
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
                            // Console.WriteLine("---\nBody: \n" + result.ToString());

                            JObject jsonResponse = JObject.Parse(result.ToString());

                            string valError = (string)jsonResponse["error"] ?? "";
                            string valErrorDesc = (string)jsonResponse["error_description"] ?? "";
                            string valAccessToken = (string)jsonResponse["access_token"] ?? "";
                            string valRefreshToken = (string)jsonResponse["refresh_token"] ?? "";

                            if (valError.Equals("") && valErrorDesc.Equals(""))
                            {
                                
                                // OK
                                this.accessToken = valAccessToken;
                                this.refreshToken = valRefreshToken;
                                this.authState = "valid";
                                Console.WriteLine("Request is valid");
                            }
                            else
                            {
                                // Bad request
                                this.authState = valError + ", " + valErrorDesc;
                                Console.WriteLine("Request is invalid");
                            }
                        }
                    }
                }
                return 1;
            }
        }
        public async Task<int> requestNewAccessTokenByRefreshToken()
        {
                        using (HttpClient client = new HttpClient())
            {
                var dict = new Dictionary<string, string>();
                dict.Add("client_id", this.clientID);
                dict.Add("client_secret", this.clientSecret);
                dict.Add("grant_type", "refresh_token");
                dict.Add("refresh_token", this.refreshToken);

                var request = new HttpRequestMessage(HttpMethod.Post, ACCESS_TOKEN_URL)
                {
                    Content = new FormUrlEncodedContent(dict)
                };

                Console.WriteLine("\nAsk for RefreshToken...");
                using (HttpResponseMessage response = await client.SendAsync(request))
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
                            // Console.WriteLine("---\nBody: \n" + result.ToString());

                            JObject jsonResponse = JObject.Parse(result.ToString());

                            string valError = (string)jsonResponse["error"] ?? "";
                            string valErrorDesc = (string)jsonResponse["error_description"] ?? "";
                            string valAccessToken = (string)jsonResponse["access_token"] ?? "";
                            string valRefreshToken = (string)jsonResponse["refresh_token"] ?? "";

                            if (valError.Equals("") && valErrorDesc.Equals(""))
                            {
                                // OK
                                this.accessToken = valAccessToken;
                                this.refreshToken = valRefreshToken;
                                this.authState = "valid";
                            }
                            else
                            {
                                // Bad request
                                this.authState = valError + ", " + valErrorDesc;
                            }
                        }
                    }
                }
                return 1;
            }
        }
        public string getAccessToken() 
        {
            return this.accessToken;
        }

        public string getAuthState() 
        {
            return this.authState;
        }
    }
}
