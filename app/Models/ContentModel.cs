using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore;

namespace app.Models
{
    public class ContentModel
    {
        public string guideBaseUrl {get; set;}
        public string reqAccessToken { get; set; }
        public string reqMethod { get; set; }
        public string reqEndPoint { get; set; }
        public string reqBody { get; set; }
        public string respStatusCode { get; set; }
        public string respBody { get; set; }

        public string partialAccessTokenAvailable {get; set;}
        
        public string partialResposeIsAvailable {get; set;}

    }
}