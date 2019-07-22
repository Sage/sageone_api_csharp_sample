using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore;

namespace app.Models
{
    public class SageOAuthModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private const string AUTHORIZE_URL = "https://www.sageone.com/oauth2/auth/central?filter=apiv3.1";        // Authorisation URL
        private const string ACCESS_TOKEN_URL = "https://oauth.accounting.sage.com/token";       // Access Token URL
        public string callbackUrl = "https://www.sageone.com/oauth2/auth/central?filter=apiv3.1&response_type=code&client_id=" + Config.ClientId + "&redirect_uri=https://localhost:5001/auth/callback&scope=full_access&state=1234567";// Config.CallbackUrl;       // Call back URL - this should match the Callback URL reigstered against your application on https://developers.sageone.com/
        private string clientID = Config.ClientId;             // Client ID - this should match the Client ID reigstered against your application on https://developers.sageone.com/
        private string clientSecret = Config.ClientSecret;     // Client Secret - this should match the Client Secret reigstered against your application on https://developers.sageone.com/
        public string authCode { get; set; }
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
        public string authState { get; set; }

        public string testVal1 { get; set; }

        public string testVal2 { get; set; }

    }
}