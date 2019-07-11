using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using Microsoft.Extensions.FileProviders;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Newtonsoft.Json.Linq;

namespace app
{
    public class HttpQueries
    {
        public static async Task<KeyValuePair<string, string>> getAccessTokenByAuthCode(String authCode)
        {   
            KeyValuePair<string,string> responseContent = new KeyValuePair<string, string>("", "");
            
            using (HttpClient client = new HttpClient())
            {
                var dict = new Dictionary<string, string>();
                dict.Add("client_id", Config.ClientId);
                dict.Add("client_secret", Config.ClientSecret);
                dict.Add("code", authCode);
                dict.Add("grant_type", "authorization_code");
                dict.Add("redirect_uri", Config.CallbackUrl);

                var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth.accounting.sage.com/token")
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

                            string valError = (string)jsonResponse["error"];
                            string valErrorDesc = (string)jsonResponse["error_description"];
                            string valAccessToken = (string)jsonResponse["access_token"];

                            if(valError.Equals("") && valErrorDesc.Equals(""))
                            {
                                // OK
                                responseContent = new KeyValuePair<string, string>(responseContent.Key, "accessToken");
                                responseContent = new KeyValuePair<string, string>(responseContent.Value, valAccessToken);
                            }
                            else
                            {
                                // Bad request
                                responseContent = new KeyValuePair<string, string>(responseContent.Key, "Error");
                                responseContent = new KeyValuePair<string, string>(responseContent.Value, valError + " - " + valErrorDesc);
                            }
                        }
                    }
                }
            }

            return responseContent;
        }
         public static async void queryCountries(String accessToken)
        {
            
        } 
    }
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddDirectoryBrowser();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseFileServer();
            app.UseStaticFiles();
            app.UseDefaultFiles();


            //app.UseHttpsRedirection();
            app.UseMvc();
            /*
                        String path = Path.Combine("/dist", "wwwroot", "images");
                        Console.WriteLine("####: " + path);

                        app.UseDirectoryBrowser(new DirectoryBrowserOptions
                        {
                            FileProvider = new PhysicalFileProvider(path),
                            RequestPath = "/MyImages"
                        });
              */
        }
    }
}
