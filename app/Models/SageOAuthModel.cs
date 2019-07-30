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
       
        public string authCode { get; set; }
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
        public string authState { get; set; }

        public int partialAccessTokenAvailable { get; set; }
        public int partialResposeIsAvailable { get; set; }

    }
}