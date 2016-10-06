using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
namespace Sage_One_Authorisation_Client.RequestHelper
{
    public class RequestHelper
    {
        private SageOneOAuth oauth = new SageOneOAuth();

        public string GetRequest(Uri uri, string token, string siteID)
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();
            string JSON = webRequest.GetData(uri, token, siteID, oauth.SigningSecret);
            return JSON;
        }

        public string PostRequest(Uri uri, string requestBody, string token, string siteID)
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();
            string JSON = webRequest.PostData(uri, requestBody, token, siteID, oauth.SigningSecret);
            return JSON;
        }

        public string PutRequest(Uri uri, string requestBody, string token, string siteID)
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();
            string JSON = webRequest.PutData(uri, requestBody, token, siteID, oauth.SigningSecret);
            return JSON;
        }

        public string DeleteRequest(Uri uri, string token, string siteID)
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();
            string JSON = webRequest.DeleteData(uri, token, siteID, oauth.SigningSecret);
            return JSON;
        }
    }
}
