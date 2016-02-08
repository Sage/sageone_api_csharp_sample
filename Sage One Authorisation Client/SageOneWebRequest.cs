using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

using System.Security.Cryptography;

namespace Sage_One_Authorisation_Client
{
    public class SageOneWebRequest
    {
        public enum Method { GET, POST, PUT, DELETE };

        public string GetData( string baseurl, string parameters, string token, string secret )
        {
            string nonce = GenerateNonce();
            string url = "";

            HttpWebRequest webRequest = null;
        
            if (parameters == "")
                url = baseurl;
            else
                url = baseurl + "?" + parameters;

            webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;

            string signingKey = GenerateSigningKey("GET", baseurl, parameters, secret, token, nonce);
            
            SetHeaders(Method.GET, webRequest, token, signingKey, nonce );


            return GetRequest(webRequest);
        }

        public string PostData(string url, string postData, string token, string secret)
        {

            try
            {
                string nonce = GenerateNonce();
                HttpWebRequest webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
                string signature = GenerateSigningKey("POST", url, postData, secret, token, nonce);

                SetHeaders(Method.POST, webRequest, token, signature, nonce);

                return SendRequest(webRequest, postData);
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

       

        public string PutData(string url, string putData, string token, string secret )
        {
            string nonce = GenerateNonce();

            HttpWebRequest webRequest = null;
            webRequest = System.Net.WebRequest.Create( url ) as HttpWebRequest;

            string signingKey = GenerateSigningKey("PUT", url, putData, secret, token, nonce);

            SetHeaders(Method.PUT, webRequest, token,signingKey,nonce);

            return SendRequest(webRequest, putData);
        }

        public string DeleteData( string baseurl, string parameters, string token, string secret)
        {
            string url = "";
            string nonce = GenerateNonce();

            HttpWebRequest webRequest = null;

            if (parameters == "")
                url = baseurl;
            else
                url = baseurl + "?" + parameters;

            webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;

            string signingKey = GenerateSigningKey("DELETE", baseurl, parameters, secret, token, nonce);
            
            SetHeaders(Method.DELETE, webRequest, token,signingKey,nonce);

            return GetRequest(webRequest);
        }

        
        private void SetHeaders ( Method method, HttpWebRequest webRequest, string accessToken,string signingKey, string nonce )
        {
            webRequest.AllowAutoRedirect = true;
            webRequest.Accept = "*/*";
            webRequest.UserAgent = "Challis Test";
            webRequest.Headers.Add("X-Signature", signingKey);
            webRequest.Headers.Add("X-Nonce", nonce);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Timeout = 100000;

            if (accessToken != "")
            {
                string authorization = String.Concat("Bearer ", accessToken);
                webRequest.Headers.Add("Authorization", authorization);
            }
            
            switch ( method )
            {
                case Method.GET: 
                    webRequest.Method = "GET";
                    break;
                                
                case Method.POST: 
                    webRequest.Method = "POST";
                    break;
                
                case Method.PUT: 
                    webRequest.Method = "PUT";
                    break;

                case Method.DELETE: 
                    webRequest.Method = "DELETE";
                    break;
            }
            
            
        }

        private string GetRequest( HttpWebRequest webRequest)
        {
            
            string responseData = "";

            responseData = GetWebResponse( webRequest );

            webRequest = null;
            
            return responseData;
        }

        private string SendRequest( HttpWebRequest webRequest, string postData )
        {                
           
           
            StreamWriter requestWriter = null;

            requestWriter = new StreamWriter(webRequest.GetRequestStream());
            
            try
            {
                requestWriter.Write(postData);                
            }
            catch
            {
                throw;
            }
            finally
            {
                requestWriter.Close();
                requestWriter = null;
            }

            return GetRequest(webRequest);
        }

        private string GetWebResponse(HttpWebRequest webRequest)
        {
            StreamReader responseReader = null;
            WebResponse response;
            string responseData = "";

            try
            {
                response = webRequest.GetResponse();
                responseReader = new StreamReader(response.GetResponseStream());
                responseData = responseReader.ReadToEnd();
            }
            catch (WebException webex)
            {
                string text;

                using (var sr = new StreamReader(webex.Response.GetResponseStream()))
                {
                    text = sr.ReadToEnd();
                }

                throw new Exception(text,webex);
                
            }
            catch (Exception ex)
            {
                string message = ex.Message; 
            }
            finally
            {
                webRequest.GetResponse().GetResponseStream().Close();
                responseReader.Close();
                responseReader = null;
            }

            return responseData;
        }

        public string GenerateSigningKey( string verb ,string url, string parameters, string secret, string token, string nonce)
        {

            string basesignaturestring = verb + "&" + PercentEncodeRfc3986(url) + "&" + PercentEncodeRfc3986(parameters) + "&" + PercentEncodeRfc3986(nonce);

            secret = PercentEncodeRfc3986(secret);
            token = PercentEncodeRfc3986(token);

            string signingkey = secret + "&" + token;

            string OAuthSignature = CreateOAuthSignature(signingkey, basesignaturestring);

            return OAuthSignature;
        }

        public string PercentEncodeRfc3986(string value)
        {
            // Start with RFC 2396 escaping by calling the .NET method to do the work.
            // This MAY sometimes exhibit RFC 3986 behavior (according to the documentation).
            // If it does, the escaping we do that follows it will be a no-op since the
            // characters we search for to replace can't possibly exist in the string.
            StringBuilder escaped = new StringBuilder(Uri.EscapeDataString(value));

            string[] UriRfc3986CharsToEscape = new[] { "!", "*", "'", "(", ")" };

            // Upgrade the escaping to RFC 3986, if necessary.
            for (int i = 0; i < UriRfc3986CharsToEscape.Length; i++)
            {
                escaped.Replace(UriRfc3986CharsToEscape[i], Uri.HexEscape(UriRfc3986CharsToEscape[i][0]));
            }

            // Return the fully-RFC3986-escaped string.
            return escaped.ToString();
        }
        
        public string GenerateNonce()
        {
            RandomNumberGenerator rng = RNGCryptoServiceProvider.Create();

            Byte[] output = new Byte[32];

            rng.GetBytes(output);

           return Convert.ToBase64String(output);       
        }

        private string CreateOAuthSignature(string signingkey, string basesignaturestring)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] hashmessage;

            byte[] keyByte = encoding.GetBytes(signingkey);
            byte[] signatureBytes = encoding.GetBytes(basesignaturestring);
            
            using (HMACSHA1 hmacsha1 = new HMACSHA1(keyByte))
            {
                hashmessage = hmacsha1.ComputeHash(signatureBytes);
            }

            return Convert.ToBase64String(hashmessage);
        }
    }
}