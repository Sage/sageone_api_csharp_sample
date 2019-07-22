using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
// using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

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

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) // identityserver
                .AddCookie(o => o.LoginPath = new PathString("/login"))
                .AddOAuth("oauth2", "Sage Accounting", o =>         // http://localhost:5000/login?authscheme=oauth2
                {
                    o.ClientId = Config.ClientId;
                    o.ClientSecret = Config.ClientSecret;
                    o.CallbackPath = new PathString("/auth/callback");
                    o.AuthorizationEndpoint = Config.AuthorizationEndpoint;
                    o.TokenEndpoint = "https://oauth.accounting.sage.com/token";
                    o.UserInformationEndpoint = "https://api.accounting.sage.com/v3.1/user";//"https://api.accounting.sage.com/v3.1/countries";
                    //o.ClaimsIssuer = "IdentityServer";
                    o.SaveTokens = true;
                    // o.UsePkce = true;
                    // Retrieving user information is unique to each provider.
                    // o.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");      
                    // o.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                    // o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email", ClaimValueTypes.Email);
                    // o.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
                    // o.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
                    // o.ClaimActions.MapJsonKey("email_verified", "email_verified");
                    // o.ClaimActions.MapJsonKey(ClaimTypes.Uri, "website");
                    // o.Scope.Add("openid");
                    // o.Scope.Add("profile");
                    // o.Scope.Add("email");

                    o.Scope.Add("full_access");
                    o.Events = new OAuthEvents
                    {
                        OnRemoteFailure = HandleOnRemoteFailure,
                        OnCreatingTicket = async context =>
                        {
                            // Get the user

                            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint); // Userinformationendpoint
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken); // Bearer
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                            response.EnsureSuccessStatusCode();

                            Console.WriteLine("***" + (string)await response.Content.ReadAsStringAsync());


                            JObject jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
                            context.RunClaimActions(jsonResponse);

                            context.HttpContext.Session.SetString("access_token", context.AccessToken);
                            context.HttpContext.Session.SetString("refresh_token", context.RefreshToken);
                            context.HttpContext.Session.SetString("token_type", context.TokenType);
                            context.HttpContext.Session.SetString("expires_at", context.ExpiresIn.ToString());

                            Console.WriteLine("<ConfigureServices -> event\n");
                            foreach (var k in context.HttpContext.Session.Keys)
                            {
                                Console.WriteLine("*** " + k.ToString() + " -> " + context.HttpContext.Session.GetString(k));
                            }
                            Console.WriteLine(">\n");

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

            app.Map("/login", signinApp =>
            {
                signinApp.Run(async context =>
                {
                    var authType = context.Request.Query["authscheme"];
                    if (!string.IsNullOrEmpty(authType))
                    {
                        // By default the client will be redirect back to the URL that issued the challenge (/login?authtype=foo),
                        // send them to the home page instead (/).
                        await context.ChallengeAsync(authType, new AuthenticationProperties() { RedirectUri = "/" });
                        return;
                    }

                    var response = context.Response;
                    response.ContentType = "text/html";
                    await response.WriteAsync("<html><body>");
                    await response.WriteAsync("Choose an authentication scheme: <br>");
                    var schemeProvider = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
                    foreach (var provider in await schemeProvider.GetAllSchemesAsync())
                    {
                        await response.WriteAsync("<a href=\"?authscheme=" + provider.Name + "\">" + (provider.DisplayName ?? "(suppressed)") + "</a><br>");
                    }
                    await response.WriteAsync("</body></html>");
                    response.HttpContext.Session.SetString("startup -> login", "hijklmn");

                    foreach (var k in response.HttpContext.Session.Keys)
                    {
                        Console.WriteLine("*** " + k.ToString() + " -> " + response.HttpContext.Session.GetString(k));
                    }
                    Console.WriteLine(">\n");

                });

            });
            // Refresh the access token
            app.Map("/refresh_token", signinApp =>
            {
                signinApp.Run(async context =>
                {
                    var response = context.Response;

                    // Setting DefaultAuthenticateScheme causes User to be set
                    // var user = context.User;

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

                        // This is what [Authorize(ActiveAuthenticationSchemes = MicrosoftAccountDefaults.AuthenticationScheme)] calls
                        // await context.ChallengeAsync(MicrosoftAccountDefaults.AuthenticationScheme);

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

                    await PrintRefreshedTokensAsync(response, payload, authProperties);

                    response.HttpContext.Session.SetString("access_token", await context.GetTokenAsync("access_token"));
                    response.HttpContext.Session.SetString("refresh_token", await context.GetTokenAsync("refresh_token"));
                    response.HttpContext.Session.SetString("token_type", await context.GetTokenAsync("token_type"));
                    response.HttpContext.Session.SetString("expires_at", await context.GetTokenAsync("expires_at"));

                    Console.WriteLine("<startup -> refresh_token");
                    foreach (var k in response.HttpContext.Session.Keys)
                    {
                        Console.WriteLine("*** " + k.ToString() + " -> " + response.HttpContext.Session.GetString(k));
                    }
                    Console.WriteLine(">\n");

                    return;
                });
            });

            app.Run(async context =>
                        {
                            // Setting DefaultAuthenticateScheme causes User to be set
                            var user = context.User;

                            // This is what [Authorize] calls
                            // var user = await context.AuthenticateAsync();

                            // This is what [Authorize(ActiveAuthenticationSchemes = MicrosoftAccountDefaults.AuthenticationScheme)] calls
                            // var user = await context.AuthenticateAsync(MicrosoftAccountDefaults.AuthenticationScheme);

                            // Deny anonymous request beyond this point.
                            if (user == null || !user.Identities.Any(identity => identity.IsAuthenticated))
                            {
                                // This is what [Authorize] calls
                                // The cookie middleware will handle this and redirect to /login
                                await context.ChallengeAsync();

                                // This is what [Authorize(ActiveAuthenticationSchemes = MicrosoftAccountDefaults.AuthenticationScheme)] calls
                                // await context.ChallengeAsync(MicrosoftAccountDefaults.AuthenticationScheme);

                                return;
                            }

                            // Display user information
                            var response = context.Response;
                            response.ContentType = "text/html";
                            await response.WriteAsync("<html><body>");
                            await response.WriteAsync("Hello " + (context.User.Identity.Name ?? "anonymous") + "<br>");
                            foreach (var claim in context.User.Claims)
                            {
                                await response.WriteAsync(claim.Type + ": " + claim.Value + "<br>");
                            }

                            await response.WriteAsync("Tokens:<br>");

                            await response.WriteAsync("Access Token: " + await context.GetTokenAsync("access_token") + "<br>");
                            await response.WriteAsync("Refresh Token: " + await context.GetTokenAsync("refresh_token") + "<br>");
                            await response.WriteAsync("Token Type: " + await context.GetTokenAsync("token_type") + "<br>");
                            await response.WriteAsync("expires_at: " + await context.GetTokenAsync("expires_at") + "<br>");
                            await response.WriteAsync("<a href=\"/logout\">Logout</a><br>");
                            await response.WriteAsync("<a href=\"/refresh_token\">Refresh Token</a><br>");
                            await response.WriteAsync("</body></html>");

                            response.HttpContext.Session.SetString("access_token", await context.GetTokenAsync("access_token"));
                            response.HttpContext.Session.SetString("refresh_token", await context.GetTokenAsync("refresh_token"));
                            response.HttpContext.Session.SetString("token_type", await context.GetTokenAsync("token_type"));
                            response.HttpContext.Session.SetString("expires_at", await context.GetTokenAsync("expires_at"));

                            Console.WriteLine("<startup -> ##################################################");
                            foreach (var k in response.HttpContext.Session.Keys)
                            {
                                Console.WriteLine("*** " + k.ToString() + " -> " + response.HttpContext.Session.GetString(k));
                            }
                            Console.WriteLine(">\n");

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

            // context.Response.Redirect("/error?FailureMessage=" + UrlEncoder.Default.Encode(context.Failure.Message));

            context.HandleResponse();
        }

        private Task<OAuthOptions> GetOAuthOptionsAsync(HttpContext context, string currentAuthType)
        {
            return Task.FromResult<OAuthOptions>(context.RequestServices.GetRequiredService<IOptionsMonitor<OAuthOptions>>().Get(currentAuthType));
        }
        private async Task PrintRefreshedTokensAsync(HttpResponse response, JObject payload, AuthenticationProperties authProperties)
        {
            response.ContentType = "text/html";
            await response.WriteAsync("<html><body>");
            await response.WriteAsync("Refreshed.<br>");
            await response.WriteAsync(HtmlEncoder.Default.Encode(payload.Root.ToString()).Replace(",", ",<br>") + "<br>");

            await response.WriteAsync("<br>Tokens:<br>");

            await response.WriteAsync("Access Token: " + authProperties.GetTokenValue("access_token") + "<br>");
            await response.WriteAsync("Refresh Token: " + authProperties.GetTokenValue("refresh_token") + "<br>");
            await response.WriteAsync("Token Type: " + authProperties.GetTokenValue("token_type") + "<br>");
            await response.WriteAsync("expires_at: " + authProperties.GetTokenValue("expires_at") + "<br>");

            await response.WriteAsync("<a href=\"/\">Home</a><br>");
            await response.WriteAsync("<a href=\"/refresh_token\">Refresh Token</a><br>");
            await response.WriteAsync("</body></html>");
        }
    }
}
