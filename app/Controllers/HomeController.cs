using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using app.Models;

namespace app.Controllers
{
  public class HomeController : Controller
  {
    ContentModel model = new ContentModel();

    public IActionResult Index()
    {
      // if access_token.json exists load it and set view settings
      this.tokenfileRead(HttpContext);

      HttpContext.Session.SetString("BaseUrl", "http://" + HttpContext.Request.Host);

      if (Startup.getPathOfConfigFile().Equals(""))
      {
          // show warning on Req.cshtml
         model.clientApplicationConfigNotFound = "1";
      }
      else
      {
        model.clientApplicationConfigNotFound = "0";
      }

      var readValue = new Byte[1024];
      if (!HttpContext.Session.TryGetValue("reqEndpoint", out readValue))
        HttpContext.Session.SetString("reqEndpoint", "contacts");


      if (!HttpContext.Session.TryGetValue("access_token", out readValue))
      {
        // access_token field is empty
        model.partialAccessTokenAvailable = "0";
        model.partialResposeIsAvailable = "0";
      }
      else if (HttpContext.Session.TryGetValue("access_token", out readValue) && !HttpContext.Session.TryGetValue("responseContent", out readValue))
      {
        // access_token filled and responseContent is empty
        model.partialAccessTokenAvailable = "1";
        model.partialResposeIsAvailable = "0";
      }
      else
      {
        model.partialAccessTokenAvailable = "1";
        model.partialResposeIsAvailable = "1";
      }

      return View(model);
    }

    public IActionResult Guide()
    {
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
      return View(model);
    }

    public void tokenfileRead(HttpContext context)
    {
      if (System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "access_token.json")))
      {
        String fs = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "access_token.json"));

        var content = JsonSerializer.Deserialize<JsonAccessTokenFile>(fs);
        context.Request.HttpContext.Session.SetString("access_token", content.access_token);
        context.Request.HttpContext.Session.SetString("expires_at", content.expires_at.ToString());
        context.Request.HttpContext.Session.SetString("refresh_token", content.refresh_token);
        context.Request.HttpContext.Session.SetString("refresh_token_expires_at", content.refresh_token_expires_at.ToString());
      }
    }
  }
}
