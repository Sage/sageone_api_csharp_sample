using System;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json.Linq;
using System.Text;
using System.IO;

namespace Sage_One_Authorisation_Client
{
    public class SageOneOAuth
    {
        private const string AUTHORIZE_URL      = "https://www.sageone.com/oauth2/auth/central";        // Authorisation URL

        private const string CA_ACCESS_TOKEN_URL   = "https://mysageone.ca.sageone.com/oauth2/token/";       // Access Token URL
        private const string US_ACCESS_TOKEN_URL = "https://mysageone.na.sageone.com/oauth2/token/";       // Access Token URL
        private const string GB_ACCESS_TOKEN_URL = "https://app.sageone.com/oauth2/token/";       // Access Token URL
        private const string IE_ACCESS_TOKEN_URL = "https://app.sageone.com/oauth2/token/";       // Access Token URL
        private const string EU_ACCESS_TOKEN_URL = "https://oauth.eu.sageone.com/token";          // Access Token URL
        private const string CALLBACK_URL       = "http://localhost:59793/callback.aspx";       // Call back URL - this should match the Callback URL reigstered against your application on https://developers.sageone.com/

        private string _clientID = "xxxxxxxxxxxxxxx"; // Client ID - this should match the Client ID reigstered against your application on https://developers.sageone.com/
        private string _clientSSecret = "xxxxxxxxxxxxxxx"; // Client Secret - this should match the Client Secret reigstered against your application on https://developers.sageone.com/
        private string _signingSecret = "xxxxxxxxxxxxxxx"; // Signing Secret - this should match the Signing Secret reigstered against your application on https://developers.sageone.com/

        private string _subscriptionKey = "xxxxxxxxxxxxxxx"; // Subscription Key - this should match the Sage One Subscription Key generated for your profile on https://developer.columbus.sage.com

        private string _token = "";
        private string _code = "";
        private string _site_id = "";
        private string _country = "";

        #region Properties

        public string SigningSecret
        {
            get 
            {
                return _signingSecret;
            }
        }

        public string AccessTokenURL
        {
            get
            {
                string country = this.Country;

                switch (country)
                {
                    case "CA":
                        return CA_ACCESS_TOKEN_URL;
                    case "US":
                        return US_ACCESS_TOKEN_URL;
                    case "GB":
                        return GB_ACCESS_TOKEN_URL;
                    case "IE":
                        return IE_ACCESS_TOKEN_URL;
                    case "DE":
                        return EU_ACCESS_TOKEN_URL;
                    case "ES":
                        return EU_ACCESS_TOKEN_URL;
                    case "FR":
                        return EU_ACCESS_TOKEN_URL;
                }

                return null;
            }
        }
        
        public string AccessTokenPostData
        { 
            get
            {                
                StringBuilder postDataBuilder = new StringBuilder();
                postDataBuilder.Append("client_id=" + this.ClientID + "&");
                postDataBuilder.Append("client_secret=" + this.ClientSecret + "&");
                postDataBuilder.Append("code=" + HttpUtility.UrlEncode(_code) + "&");
                postDataBuilder.Append("grant_type=authorization_code&");
                postDataBuilder.Append("redirect_uri=" + HttpUtility.UrlEncode(CALLBACK_URL));
                return postDataBuilder.ToString();
            }
        }

        public string AuthorizationURL
        {
            get
            {
                return string.Format("{0}?response_type=code&client_id={1}&redirect_uri={2}&scope=full_access", AUTHORIZE_URL, this.ClientID, HttpUtility.UrlEncode(CALLBACK_URL)); 
            }
        }

        public string ClientID
        {
            get
            {
                return _clientID;
            }
        }

        public string ClientSecret 
        {
            get 
            {           
                return _clientSSecret;
            }
        }

        public string Token 
        {   
            get 
            { 
                return _token; 
            } 
            set 
            { 
                _token = value; 
            } 
        }

        public string SubscriptionKey
        {
            get
            {
                return _subscriptionKey;
            }           
        }
             

        public string SiteID
        {
            get
            {
                return _site_id;
            }
            set
            {
                _site_id = value;
            }
        }

        public string Country
        {
            get
            {
                return _country;
            }
            set
            {
                _country = value;
            }
        }
        #endregion

        /// <summary>
        /// Exchange the authorisation code for an access token.
        /// </summary>
        /// <param name="code">The code supplied by Sage One's authorization page following the callback.</param>
        public void GetAccessToken( string code, string country )
        {
            this.Country = country;
            SageOneWebRequest request = new SageOneWebRequest();
            _code = code;
            
            string postData = AccessTokenPostData;
            Uri accesstokenURI = new Uri(AccessTokenURL);
            string response = request.PostData(accesstokenURI, postData, "", "", "");
            
            if (response.Length > 0)
            {
                JObject jObject = JObject.Parse(response);
                string access_token = (string) jObject["access_token"];
                string site_id = (string)jObject["resource_owner_id"];

                if (access_token != null)
                {
                    this.Token = access_token;
                }

                if (site_id != null)
                {
                    this.SiteID = site_id;
                }
            }
        }
     }
}
