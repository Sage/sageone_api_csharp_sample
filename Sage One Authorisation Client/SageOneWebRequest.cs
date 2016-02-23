using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Web;
using System.Security.Cryptography;

namespace Sage_One_Authorisation_Client
{
    public class SageOneWebRequest
    {
        public enum Method { GET, POST, PUT, DELETE };

        public string GetData( Uri url, string token, string signingSecret )
        {
            // Create the nonce to be used by the request
            string nonce = GenerateNonce();      
            
            // Create the web request            
            HttpWebRequest webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;

            // Generate a signature
            string signature = SageOneAPIRequestSigner.GenerateSignature("GET", url, null, signingSecret, token, nonce);
            
            // Set the request headers
            SetHeaders(Method.GET, webRequest, token, signature, nonce );

            // Send the GET request
            return GetRequest(webRequest);
        }

        public string PostData(Uri url, List<KeyValuePair<string, string>> requestBody, string token, string signingSecret)
        {
            try
            {
                // Create the nonce to be used by the request
                string nonce = GenerateNonce();

                // Create the web request 
                HttpWebRequest webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;

                // Generate a signature
                string signature = SageOneAPIRequestSigner.GenerateSignature("POST", url, requestBody, signingSecret, token, nonce);

                // Set the request headers
                SetHeaders(Method.POST, webRequest, token, signature, nonce);

                // Convert the requestBody into post parameters 
                string postParams = ConvertPostParams(requestBody);

                // Send the POST request
                return SendRequest(webRequest, postParams);
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

       

        public string PutData(Uri url, List<KeyValuePair<string, string>> requestBody, string token, string signingSecret )
        {
            // Create the nonce to be used by the request
            string nonce = GenerateNonce();

            // Create the web request 
            HttpWebRequest webRequest = System.Net.WebRequest.Create( url ) as HttpWebRequest;

            // Generate a signature
            string signature = SageOneAPIRequestSigner.GenerateSignature("PUT", url, requestBody, signingSecret, token, nonce);

            // Set the request headers
            SetHeaders(Method.PUT, webRequest, token, signature, nonce);

            // Convert the requestBody into put parameters
            string putParams = ConvertPostParams(requestBody);

            // Send the PUT request
            return SendRequest(webRequest, putParams);
        }

        public string DeleteData( Uri baseurl, string token, string signingSecret)
        {
            // Create the nonce to be used by the request
            string nonce = GenerateNonce();

            // Create the web request 
            HttpWebRequest webRequest = System.Net.WebRequest.Create(baseurl) as HttpWebRequest;

            // Generate a signature
            string signature = SageOneAPIRequestSigner.GenerateSignature("DELETE", baseurl, null, signingSecret, token, nonce);

            // Set the request headers
            SetHeaders(Method.DELETE, webRequest, token, signature, nonce);

            // Send the DELETE request
            return GetRequest(webRequest);
        }

        
        private void SetHeaders ( Method method, HttpWebRequest webRequest, string accessToken,string signature, string nonce )
        {
            // Set the required header values on the web request
            webRequest.AllowAutoRedirect = true;
            webRequest.Accept = "*/*";
            webRequest.UserAgent = "CSharp Test";
            webRequest.Headers.Add("X-Signature", signature);
            webRequest.Headers.Add("X-Nonce", nonce);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Timeout = 100000;

            // pass the current access token as a header parameter
            if (accessToken != "")
            {
                string authorization = String.Concat("Bearer ", accessToken);
                webRequest.Headers.Add("Authorization", authorization);
            }
            
            // Set the request method verb
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

              
        public string GenerateNonce()
        {
            RandomNumberGenerator rng = RNGCryptoServiceProvider.Create();
            Byte[] output = new Byte[32];
            rng.GetBytes(output);
            return Convert.ToBase64String(output);       
        }
       
        private string ConvertPostParams(List<KeyValuePair<string, string>> requestBody)
        {
            IEnumerable<KeyValuePair<string, string>> kvpParams = requestBody;
            // Sort the parameters
            IEnumerable<string> sortedParams =
              from p in requestBody              
              select p.Key + "=" + p.Value;

            // Add the ampersand delimiter and then URL-encode
            string encodedParams = String.Join("&", sortedParams);            
            return encodedParams;

        }
    }
}