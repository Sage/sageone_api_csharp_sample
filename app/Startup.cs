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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.IO;

namespace app
{
  public class JsonResponse
  {
        public String access_token  { get; set; }
        public long expires_in { get; set; }
        public String bearer { get; set; }
        public String refresh_token { get; set; }
        public long refresh_token_expires_in { get; set; }
        public String scope { get; set; }
        public String requested_by_id { get; set; }
  }
  public class JsonAccessTokenFile
  {
        public String access_token  { get; set; }
        public long expires_at { get; set; }
        public String refresh_token { get; set; }
        public long refresh_token_expires_at { get; set; }
  }
  public class JsonClientApplicationFile
  {
        public JsonClientApplicationFileConfigSection config { get; set; }
  }
  public class JsonClientApplicationFileConfigSection
  {
        public String client_id  { get; set; }
        public String client_secret { get; set; }
        public String callback_url { get; set; }
  }
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }
    const string API_URL = "https://api.accounting.sage.com/v3.1/";
    const string AUTHORIZATION_ENDPOINT = "https://www.sageone.com/oauth2/auth/central?filter=apiv3.1";
    const string TOKEN_ENDPOINT = "https://oauth.accounting.sage.com/token";

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {

      String config_client_id = "initial";
      String config_client_secret = "initial";
      String config_calback_url = "initial";

      if (!(getPathOfConfigFile().Equals("")))
      {
          String fs = File.ReadAllText(getPathOfConfigFile());
          {
              var content = System.Text.Json.JsonSerializer.Deserialize<JsonClientApplicationFile>(fs);
              config_client_id = content.config.client_id;
              config_client_secret = content.config.client_secret;
              config_calback_url = content.config.callback_url;
          }
      }

      services.AddDistributedMemoryCache();
      services.AddSession(options =>
      {
        options.Cookie.HttpOnly = false;
        options.Cookie.IsEssential = true;
        options.IdleTimeout = TimeSpan.FromHours(1);
      });

      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
      services.AddRazorPages();
      services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
          .AddCookie(o => o.LoginPath = new PathString("/login"))
          .AddOAuth("oauth2", "Sage Accounting", o =>
          {
            o.ClientId = config_client_id;
            o.ClientSecret = config_client_secret;
            o.CallbackPath = new PathString("/auth/callback");
            o.AuthorizationEndpoint = AUTHORIZATION_ENDPOINT;
            o.TokenEndpoint = TOKEN_ENDPOINT;
            o.SaveTokens = true;

            o.Scope.Add("full_access");
            o.Events = new OAuthEvents
            {
              OnRemoteFailure = HandleOnRemoteFailure,
              OnCreatingTicket = async context =>
              {
                  long tok_expires_in = context.TokenResponse.Response.RootElement.GetProperty("expires_in").GetInt64();
                  long tok_refresh_token_expires_in = context.TokenResponse.Response.RootElement.GetProperty("refresh_token_expires_in").GetInt64();

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
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
      app.UseRouting();
      app.UseStaticFiles();
      app.UseSession();
      app.UseCookiePolicy();
      app.UseAuthentication();
      app.UseEndpoints(endpoints => endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}"));

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

                var jsonResponse = JsonSerializer.Deserialize<JsonResponse>((string)await refreshResponse.Content.ReadAsStringAsync());

                authProperties.UpdateTokenValue("access_token", jsonResponse.access_token);
                authProperties.UpdateTokenValue("refresh_token", jsonResponse.refresh_token);

                var expiresAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(jsonResponse.expires_in);
                authProperties.UpdateTokenValue("expires_at", expiresAt.ToString("o", CultureInfo.InvariantCulture));

                await context.SignInAsync(user, authProperties);

                // write new tokens and times to file
                tokenfileWrite(jsonResponse.access_token,
                                calculateUnixtimestampWithOffset(jsonResponse.expires_in),
                                jsonResponse.refresh_token,
                                calculateUnixtimestampWithOffset(jsonResponse.refresh_token_expires_in),
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

                   System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                   timer.Start();

                   using (HttpClient client = new HttpClient())
                   {
                     client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Response.HttpContext.Session.GetString("access_token"));
                     client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                     HttpResponseMessage request = null;

                     if (qry_http_verb.Equals("get"))
                     {
                       request = await client.GetAsync(API_URL + qry_resource);
                     }
                     else if (qry_http_verb.Equals("post"))
                     {
                       request = await client.PostAsync(API_URL + qry_resource, new StringContent(qry_post_data, Encoding.UTF8, "application/json"));
                     }
                     else if (qry_http_verb.Equals("put"))
                     {
                       request = await client.PutAsync(API_URL + qry_resource, new StringContent(qry_post_data, Encoding.UTF8, "application/json"));
                     }
                     else if (qry_http_verb.Equals("delete"))
                     {
                       request = await client.DeleteAsync(API_URL + qry_resource);
                     }

                     Task<string> respContent = request.Content.ReadAsStringAsync();
                     Task.WaitAll(respContent);

                     dynamic parsedJson = Newtonsoft.Json.JsonConvert.DeserializeObject(respContent.Result.ToString());
                     String responseContentPretty = Newtonsoft.Json.JsonConvert.SerializeObject(parsedJson, Newtonsoft.Json.Formatting.Indented);

                     context.Response.HttpContext.Session.SetString("responseStatusCode", (int)request.StatusCode + " - " + request.StatusCode.ToString());
                     context.Response.HttpContext.Session.SetString("reqEndpoint", qry_resource);

                     context.Response.HttpContext.Session.SetString("responseContent", responseContentPretty);
                     context.Response.HttpContext.Session.SetString("responseTimespan", timer.Elapsed.Seconds + "." + timer.Elapsed.Milliseconds);

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

    #region helper
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
      JsonAccessTokenFile content = new JsonAccessTokenFile();

      content.access_token = access_token;
      content.expires_at = expires_at;
      content.refresh_token = refresh_token;
      content.refresh_token_expires_at = refresh_token_expires_at;

      var options = new JsonSerializerOptions { WriteIndented = true };

      var newContent = JsonSerializer.Serialize(content, options);

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

        String fs = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "access_token.json"));

        var content = JsonSerializer.Deserialize<JsonAccessTokenFile>(fs);
        context.Request.HttpContext.Session.SetString("access_token", content.access_token);
        contentFromFile.Add("access_token", content.access_token);

        context.Request.HttpContext.Session.SetString("expires_at", content.expires_at.ToString());
        contentFromFile.Add("expires_at",  content.expires_at.ToString());

        context.Request.HttpContext.Session.SetString("refresh_token", content.refresh_token);
        contentFromFile.Add("refresh_token", content.refresh_token);

        context.Request.HttpContext.Session.SetString("refresh_token_expires_at", content.refresh_token_expires_at.ToString());
        contentFromFile.Add("refresh_token_expires_at", content.refresh_token_expires_at.ToString());
      }
      return contentFromFile;
    }

    public static long calculateUnixtimestampWithOffset(long offset = 0)
    {
      long seconds = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds + offset;

      return seconds;
    }

    public static String getPathOfConfigFile()
    {

      if (System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "client_application.json")))
      {
        return Path.Combine(Directory.GetCurrentDirectory(), "client_application.json");
      }
      else if (System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "app/client_application.json")))
      {
        return Path.Combine(Directory.GetCurrentDirectory(), "app/client_application.json");
      }

      return "";
    }
  }
  #endregion
}
