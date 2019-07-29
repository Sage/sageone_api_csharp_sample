using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace app
{
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

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = false;

                options.IdleTimeout = TimeSpan.FromMinutes(15);
                //options.Cookie.SameSite = SameSiteMode.Strict;
                //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.AddMvc();//.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) // identityserver
                .AddCookie(o => o.LoginPath = new PathString("/login"))
                .AddOAuth("oauth2", "Sage Accounting", o =>
                {
                    o.ClientId = Config.ClientId;
                    o.ClientSecret = Config.ClientSecret;
                    o.CallbackPath = new PathString("/auth/callback");
                    o.AuthorizationEndpoint = Config.AuthorizationEndpoint;
                    o.TokenEndpoint = "https://oauth.accounting.sage.com/token";
                    o.UserInformationEndpoint = "https://api.accounting.sage.com/v3.1/user";//"https://api.accounting.sage.com/v3.1/countries";
                    o.SaveTokens = true;

                    o.Scope.Add("full_access");
                    o.Events = new OAuthEvents
                    {
                        OnRemoteFailure = HandleOnRemoteFailure,
                        OnCreatingTicket = async context =>
                        {
                            // Get the user

                            context.HttpContext.Session.SetString("access_token", context.AccessToken);
                            context.HttpContext.Session.SetString("refresh_token", context.RefreshToken);
                            context.HttpContext.Session.SetString("token_type", context.TokenType);
                            context.HttpContext.Session.SetString("expires_at", context.ExpiresIn.ToString());

/*                             Console.WriteLine("<ConfigureServices -> event\n");
                            foreach (var k in context.HttpContext.Session.Keys)
                            {
                                Console.WriteLine("*** " + k.ToString() + " -> " + context.HttpContext.Session.GetString(k));
                            }
                            Console.WriteLine(">\n"); */
                        }
                    };
                });
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();

            app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // get access token
            app.Map("/login", signinApp =>
               {
                   signinApp.Run(async context =>
                   {
                       await context.ChallengeAsync("oauth2", new AuthenticationProperties() { RedirectUri = "/" });
                       return;
                   });

               });

            // Refresh the access token
            app.Map("/refresh_token", signinApp =>
            {
                signinApp.Run(async context =>
                {
                    var response = context.Response;

                    // This is what [Authorize] calls
                    var userResult = await context.AuthenticateAsync();
                    var user = userResult.Principal;
                    var authProperties = userResult.Properties;

                    // Deny anonymous request beyond this point.
                    if (!userResult.Succeeded || user == null || !user.Identities.Any(identity => identity.IsAuthenticated))
                    {
                        // This is what [Authorize] calls
                        // The cookie middleware will handle this and redirect to /login
                        await context.ChallengeAsync();

                        return;
                    }

                    var currentAuthType = user.Identities.First().AuthenticationType;
                    var refreshToken = authProperties.GetTokenValue("refresh_token");

                    if (string.IsNullOrEmpty(refreshToken))
                    {
                        response.ContentType = "text/html";
                        await response.WriteAsync("<html><body>");
                        await response.WriteAsync("No refresh_token is available.<br>");
                        await response.WriteAsync("<a href=\"/\">Home</a>");
                        await response.WriteAsync("</body></html>");
                        return;
                    }

                    var options = await GetOAuthOptionsAsync(context, currentAuthType);

                    var pairs = new Dictionary<string, string>()
                        {
                            { "client_id", options.ClientId },
                            { "client_secret", options.ClientSecret },
                            { "grant_type", "refresh_token" },
                            { "refresh_token", refreshToken }
                        };
                    var content = new FormUrlEncodedContent(pairs);
                    var refreshResponse = await options.Backchannel.PostAsync(options.TokenEndpoint, content, context.RequestAborted);
                    refreshResponse.EnsureSuccessStatusCode();

                    JObject payload = JObject.Parse((string)await refreshResponse.Content.ReadAsStringAsync());

                    // Persist the new acess token
                    authProperties.UpdateTokenValue("access_token", (string)payload["access_token"]);
                    refreshToken = (string)payload["refresh_token"];
                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        authProperties.UpdateTokenValue("refresh_token", refreshToken);
                    }
                    if (payload.TryGetValue("expires_in", out var property))
                    {
                        int seconds = (int)payload["expires_in"];
                        var expiresAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(seconds);
                        authProperties.UpdateTokenValue("expires_at", expiresAt.ToString("o", CultureInfo.InvariantCulture));
                    }
                    await context.SignInAsync(user, authProperties);

                    response.HttpContext.Session.SetString("access_token", await context.GetTokenAsync("access_token"));
                    response.HttpContext.Session.SetString("refresh_token", await context.GetTokenAsync("refresh_token"));
                    response.HttpContext.Session.SetString("token_type", await context.GetTokenAsync("token_type"));
                    response.HttpContext.Session.SetString("expires_at", await context.GetTokenAsync("expires_at"));


                    context.Response.Redirect(Config.BaseUrl + "/");

                    return;
                });
            });

            app.Map("/query_api", signinApp =>
               {
                   signinApp.Run(async context =>
                   {
                      String qry_http_verb =  context.Request.Query["http_verb"].ToString() ?? "";
                      String qry_resource =  context.Request.Query["resource"].ToString() ?? "";
                      String qry_post_data =  context.Request.Query["post_data"].ToString() ?? "";
                    
                      Console.WriteLine(qry_http_verb + " - " + qry_resource + " - " + qry_post_data);

                    using (HttpClient client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Response.HttpContext.Session.GetString("access_token"));
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            
                             HttpRequestMessage request;

                              switch (qry_http_verb)
                              {
                                  case "get":
                                      request = new HttpRequestMessage(HttpMethod.Get, Config.ApiBaseEndpoint + qry_resource);
                                      break;
                                  case "post":
                                      request = new HttpRequestMessage(HttpMethod.Post, Config.ApiBaseEndpoint + qry_resource);
                                      break;
                                  case "put":
                                      request = new HttpRequestMessage(HttpMethod.Put, Config.ApiBaseEndpoint + qry_resource);
                                      break;
                                  case "delete":
                                      request = new HttpRequestMessage(HttpMethod.Delete, Config.ApiBaseEndpoint + qry_resource);
                                      break;
                                  default:
                                      request = new HttpRequestMessage(HttpMethod.Get, Config.ApiBaseEndpoint + qry_resource);
                                      break;
                              }

                            if (qry_http_verb.Equals("post") && !qry_post_data.Equals(""))
                            {
                                Console.WriteLine("Post und body");
                                request.Content = new ByteArrayContent(Encoding.ASCII.GetBytes(qry_post_data));
                            }

                            Task<HttpResponseMessage> responseMsgAsync = client.SendAsync(request);
                            Task.WaitAll(responseMsgAsync);

                            HttpResponseMessage response = responseMsgAsync.Result;
                            //{
                                using (HttpContent content = response.Content)
                                {
                                    string result = await content.ReadAsStringAsync();

                                    context.Response.HttpContext.Session.SetString("responseStatusCode", response.StatusCode.ToString());
                                    context.Response.HttpContext.Session.SetString("reqEndpoint", qry_resource);

                                    if (result != null &&
                                        result.Length >= 50)
                                    {
                                      dynamic parsedJson = JsonConvert.DeserializeObject(result.ToString());
                                      String responseContentPretty =  JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                                      context.Response.HttpContext.Session.SetString("responseContent", responseContentPretty);

                                    }
                                }
                            //}
                        }

                      context.Response.Redirect(Config.BaseUrl + "/");

                      return;
                   });

               });

            app.Run(async context =>
                        {
                            // Setting DefaultAuthenticateScheme causes User to be set
                            var user = context.User;

                            // Deny anonymous request beyond this point.
                            if (user == null || !user.Identities.Any(identity => identity.IsAuthenticated))
                            {
                                // This is what [Authorize] calls
                                // The cookie middleware will handle this and redirect to /login
                                await context.ChallengeAsync();

                                return;
                            }

                        });


            // Sign-out to remove the user cookie.
            app.Map("/logout", signoutApp =>
            {
                signoutApp.Run(async context =>
                {
                    var response = context.Response;
                    response.ContentType = "text/html";
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await response.WriteAsync("<html><body>");
                    await response.WriteAsync("You have been logged out. Goodbye " + context.User.Identity.Name + "<br>");
                    await response.WriteAsync("<a href=\"/\">Home</a>");
                    await response.WriteAsync("</body></html>");
                });
            });

            // Display the remote error
            app.Map("/error", errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var response = context.Response;
                    response.ContentType = "text/html";
                    await response.WriteAsync("<html><body>");
                    await response.WriteAsync("An remote failure has occurred: " + context.Request.Query["FailureMessage"] + "<br>");
                    await response.WriteAsync("<a href=\"/\">Home</a>");
                    await response.WriteAsync("</body></html>");
                });
            });
        }

        private async Task HandleOnRemoteFailure(RemoteFailureContext context)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync("<html><body>");
            await context.Response.WriteAsync("A remote failure has occurred: <br>" +
                context.Failure.Message.Split(Environment.NewLine).Select(s => HtmlEncoder.Default.Encode(s) + "<br>").Aggregate((s1, s2) => s1 + s2));

            if (context.Properties != null)
            {
                await context.Response.WriteAsync("Properties:<br>");
                foreach (var pair in context.Properties.Items)
                {
                    await context.Response.WriteAsync($"-{ HtmlEncoder.Default.Encode(pair.Key)}={ HtmlEncoder.Default.Encode(pair.Value)}<br>");
                }
            }

            await context.Response.WriteAsync("<a href=\"/\">Home</a>");
            await context.Response.WriteAsync("</body></html>");

            context.HandleResponse();
        }

        private Task<OAuthOptions> GetOAuthOptionsAsync(HttpContext context, string currentAuthType)
        {
            return Task.FromResult<OAuthOptions>(context.RequestServices.GetRequiredService<IOptionsMonitor<OAuthOptions>>().Get(currentAuthType));
        }
    }
}
