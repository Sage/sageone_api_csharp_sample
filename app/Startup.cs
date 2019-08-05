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
using System.IO;
using Microsoft.Extensions.Configuration.Json;

namespace app
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }
    const string API_URL = "https://api.accounting.sage.com/v3.1/";

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {

      String config_client_id = "initial";
      String config_client_secret = "initial";
      String config_calback_url = "initial";

      if (!(getPathOfConfigFile().Equals("")))
        using (StreamReader file = File.OpenText(getPathOfConfigFile()))
        using (JsonTextReader reader = new JsonTextReader(file))
        {
          JObject configObj = (JObject)JToken.ReadFrom(reader);
          config_client_id = (string)configObj["config"]["client_id"];
          config_client_secret = (string)configObj["config"]["client_secret"];
          config_calback_url = (string)configObj["config"]["callback_url"];
        }


      services.AddDistributedMemoryCache();
      services.AddSession(options =>
      {
        options.Cookie.HttpOnly = false;
        options.Cookie.IsEssential = true;
        options.IdleTimeout = TimeSpan.FromHours(1);
      });

      services.AddMvc();//.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

      services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
          .AddCookie(o => o.LoginPath = new PathString("/login"))
          .AddOAuth("oauth2", "Sage Accounting", o =>
          {
            o.ClientId = config_client_id;
            o.ClientSecret = config_client_secret;
            o.CallbackPath = new PathString("/auth/callback");
            o.AuthorizationEndpoint = "https://www.sageone.com/oauth2/auth/central?filter=apiv3.1";
            o.TokenEndpoint = "https://oauth.accounting.sage.com/token";
            o.SaveTokens = true;

            o.Scope.Add("full_access");
            o.Events = new OAuthEvents
            {
              OnRemoteFailure = HandleOnRemoteFailure,
              OnCreatingTicket = async context => //async
                    {
/* 
                  Console.WriteLine(">>>>" +  (string)context.TokenResponse.Response["expires_in"]);
                  Console.WriteLine(">>>>" +  (string)context.TokenResponse.Response["refresh_token_expires_in"]); */

                  int tok_expires_in = (int) context.TokenResponse.Response["expires_in"];
                  int tok_refresh_token_expires_in = (int) context.TokenResponse.Response["refresh_token_expires_in"];

                      tokenfileWrite(context.AccessToken, 
                                     calculateUnixtimestampWithOffset(tok_expires_in), 
                                     context.RefreshToken, 
                                     calculateUnixtimestampWithOffset(tok_refresh_token_expires_in), 
                                     context.HttpContext);
                      return;
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
        app.UseHsts();
      }
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

                int tok_expires_in = Int32.Parse((string)payload["expires_in"]);
                int tok_refresh_token_expires_in = Int32.Parse((string)payload["refresh_token_expires_in"]);
                
                // Persist the new acess token to the properties-object
                authProperties.UpdateTokenValue("access_token", (string)payload["access_token"]);
                authProperties.UpdateTokenValue("refresh_token", (string)payload["refresh_token"]);

                if (payload.TryGetValue("expires_in", out var property))
                {
                  int seconds = (int)payload["expires_in"];
                  var expiresAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(seconds);
                  authProperties.UpdateTokenValue("expires_at", expiresAt.ToString("o", CultureInfo.InvariantCulture));
                }

                await context.SignInAsync(user, authProperties);

                // write new tokens and times to file
                tokenfileWrite(await context.GetTokenAsync("access_token"),
                                calculateUnixtimestampWithOffset(tok_expires_in),
                                await context.GetTokenAsync("refresh_token"),
                                calculateUnixtimestampWithOffset(tok_refresh_token_expires_in),
                                context);

                context.Response.Redirect("/");

                return;
              });
      });

      app.Map("/query_api", signinApp =>
         {
           signinApp.Run(async context =>
                 {
                   String qry_http_verb = context.Request.Query["http_verb"].ToString() ?? "";
                   String qry_resource = context.Request.Query["resource"].ToString() ?? "";
                   String qry_post_data = context.Request.Query["post_data"].ToString() ?? "";

                   Console.WriteLine("/query_api -> " + qry_http_verb + " -> " + qry_resource + " -> " + qry_post_data);

                   System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                   timer.Start();

                   using (HttpClient client = new HttpClient())
                   {
                     client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Response.HttpContext.Session.GetString("access_token"));
                     client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                     HttpRequestMessage request;

                     switch (qry_http_verb)
                     {
                       case "get":
                         request = new HttpRequestMessage(HttpMethod.Get, API_URL + qry_resource);
                         break;
                       case "post":
                         request = new HttpRequestMessage(HttpMethod.Post, API_URL + qry_resource);
                         break;
                       case "put":
                         request = new HttpRequestMessage(HttpMethod.Put, API_URL + qry_resource);
                         break;
                       case "delete":
                         request = new HttpRequestMessage(HttpMethod.Delete, API_URL + qry_resource);
                         break;
                       default:
                         request = new HttpRequestMessage(HttpMethod.Get, API_URL + qry_resource);
                         break;
                     }

                     // if post or put is selected and Request Body is not empty, set content 
                     if ((qry_http_verb.Equals("post") || qry_http_verb.Equals("put")) && !qry_post_data.Equals(""))
                     {
                       request.Content = new ByteArrayContent(Encoding.ASCII.GetBytes(qry_post_data));
                     }

                     Task<HttpResponseMessage> responseMsgAsync = client.SendAsync(request);
                     Task.WaitAll(responseMsgAsync);

                     HttpResponseMessage response = responseMsgAsync.Result;
                     using (HttpContent content = response.Content)
                     {
                       string result = await content.ReadAsStringAsync();

                       context.Response.HttpContext.Session.SetString("responseStatusCode", (int)response.StatusCode + " - " + response.StatusCode.ToString());
                       context.Response.HttpContext.Session.SetString("reqEndpoint", qry_resource);

                       timer.Stop();
                       if (result != null &&
                                 result.Length >= 50)
                       {
                         // prettify json
                         dynamic parsedJson = JsonConvert.DeserializeObject(result.ToString());
                         String responseContentPretty = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                         context.Response.HttpContext.Session.SetString("responseContent", responseContentPretty);
                         context.Response.HttpContext.Session.SetString("responseTimespan", timer.Elapsed.Seconds + "." + timer.Elapsed.Milliseconds);
                       }
                     }
                   }

                   context.Response.Redirect("/");
                   return;
                 });

         });

      app.Run(async context =>
                 {
                   tokenfileRead(context);

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

    // helper
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

    public string tokenfileWrite(string access_token, long expires_at, string refresh_token, long refresh_token_expires_at, HttpContext context)
    {
      Console.WriteLine("refresh expires: " + refresh_token_expires_at);
      JObject newContent = new JObject(
        new JProperty("access_token", access_token),
        new JProperty("expires_at", expires_at),
        new JProperty("refresh_token", refresh_token),
        new JProperty("refresh_token_expires_at", refresh_token_expires_at)
        );

      File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "access_token.json"), newContent.ToString());

      context.Request.HttpContext.Session.SetString("access_token", access_token);
      context.Request.HttpContext.Session.SetString("expires_at", expires_at.ToString());
      context.Request.HttpContext.Session.SetString("refresh_token", refresh_token);
      context.Request.HttpContext.Session.SetString("refresh_token_expires_at", refresh_token_expires_at.ToString());

      return "0";
    }

    public static Dictionary<string, string> tokenfileRead(HttpContext context)
    {
      Dictionary<string, string> contentFromFile = new Dictionary<string, string>();

      if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "access_token.json")))
      {
        using (StreamReader file = File.OpenText(Path.Combine(Directory.GetCurrentDirectory(), "access_token.json")))
        using (JsonTextReader reader = new JsonTextReader(file))
        {
          JObject jsonObj = (JObject)JToken.ReadFrom(reader);
          context.Request.HttpContext.Session.SetString("access_token", (string)jsonObj["access_token"]);
          contentFromFile.Add("access_token", (string)jsonObj["access_token"]);

          context.Request.HttpContext.Session.SetString("expires_at", (string)jsonObj["expires_at"]);
          contentFromFile.Add("expires_at", (string)jsonObj["expires_at"]);

          context.Request.HttpContext.Session.SetString("refresh_token", (string)jsonObj["refresh_token"]);
          contentFromFile.Add("refresh_token", (string)jsonObj["refresh_token"]);

          context.Request.HttpContext.Session.SetString("refresh_token_expires_at", (string)jsonObj["refresh_token_expires_at"]);
          contentFromFile.Add("refresh_token_expires_at", (string)jsonObj["refresh_token_expires_at"]);

        }

      }
      return contentFromFile;
    }

    public static long calculateUnixtimestampWithOffset(int offset = 0)
    {
      long seconds = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds + offset;

      return seconds;
    }

    public static String getPathOfConfigFile()
    {

      if (System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "client_application.json")))
      {
        Console.WriteLine("use config client_application.json");
        return Path.Combine(Directory.GetCurrentDirectory(), "client_application.json");
      }
      else if (System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "app/client_application.json")))
      {
        Console.WriteLine("use config app/client_application.json");
        return Path.Combine(Directory.GetCurrentDirectory(), "app/client_application.json");
      }

      Console.WriteLine("no client_application.json found, please create one from template.");
      return "";
    }
  }
}
