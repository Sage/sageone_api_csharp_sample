using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sage_One_Authorisation_Client
{
    public class SageOneAPIRequestSigner
    {
        /// <summary>
        /// Generates the signature from all the parameters passed in
        /// </summary>
        /// <param name="httpMethod">The HTTP method</param>
        /// <param name="url">The URL</param>
        /// <param name="requestBody">The request message body (this will be null on a GET or DELETE)</param>
        /// <param name="signingKey">Your developer signing key</param>
        /// <param name="nonce">The nonce</param>
        /// <returns>The signature as a string</returns>
        public static string GenerateSignature(string httpMethod, Uri url, List<KeyValuePair<string, string>> requestBody, string signingSecret, string token, string nonce)
        {
            // Uppercase the http method e.g. GET
            httpMethod = httpMethod.ToUpper();

            // Create an encoded string that contains the concatenated values of the Method, URL and request body
            string encodedParams = NormalizeParams(httpMethod, url, requestBody);

            // Make the URL lowercase and Escape the string
            string encodedUri = Uri.EscapeDataString(url.GetLeftPart(UriPartial.Path).ToLower()); // this needs to be ToLower() to match the request check at the server

            // Escape the nonce
            string encodedNonce = Uri.EscapeDataString(nonce);

            string signingKey = Uri.EscapeDataString(signingSecret) + "&" + Uri.EscapeDataString(token);

            // Build the signature base string to be signed with the Consumer Secret (Developer Key)
            string baseString = String.Format("{0}&{1}&{2}&{3}", httpMethod, encodedUri, encodedParams, encodedNonce);

            // Generate a hash based message authentication code based on the
            // signing key and the entire message that has been parameterised and escaped
            return GenerateHmac(signingKey, baseString);

        }


        /// <summary>
        /// Generates a hash based message authentication code based on the signing key and
        /// the entire message that has been parameterised and escaped
        /// </summary>
        /// <param name="signingKey">The developer signing key</param>
        /// <param name="baseString">The entire message that has been parameterised and escaped</param>
        /// <returns>The signature representing the message</returns>
        private static string GenerateHmac(string signingKey, string baseString)
        {
            HMACSHA1 hasher = new HMACSHA1(new ASCIIEncoding().GetBytes(signingKey));

            return Convert.ToBase64String(
                hasher.ComputeHash(
                new ASCIIEncoding().GetBytes(baseString)));
        }

        /// <summary>
        /// Creates a string that represents the HTTP method, the URL and the request body
        /// </summary>
        /// <param name="httpMethod">The HTTP method</param>
        /// <param name="url">The URL</param>
        /// <param name="requestBody">The request body</param>
        /// <returns>Am escaped and alphabetically sorted string representing the parameters</returns>
        private static string NormalizeParams(string httpMethod, Uri url, List<KeyValuePair<string, string>> requestBody)
        {
            // Create a List of type KeyValuePair to contain the parameters
            IEnumerable<KeyValuePair<string, string>> kvpParams = new List<KeyValuePair<string, string>>();

            if (!string.IsNullOrWhiteSpace(url.Query))
            {
                // Extact all the query parameters from the url
                IEnumerable<KeyValuePair<string, string>> queryParams =
                  from p in url.Query.Substring(1).Split('&').AsEnumerable()
                  let key = Uri.EscapeDataString(p.Substring(0, p.IndexOf("=")))
                  let value = Uri.EscapeDataString(p.Substring(p.IndexOf("=") + 1))
                  select new KeyValuePair<string, string>(key, value);

                // Add each parameter and value found to the List of paramters
                kvpParams = kvpParams.Union(queryParams);
            }

            //Process the body parameters
            if (httpMethod == "POST" || httpMethod == "PUT")
            {
                List<KeyValuePair<string, string>> encodedrequestBodyParams = new List<KeyValuePair<string, string>>();

                foreach (KeyValuePair<string, string> pair in requestBody)
                {
                    encodedrequestBodyParams.Add(new KeyValuePair<string, string>(Uri.EscapeDataString(pair.Key), Uri.EscapeDataString(pair.Value)));
                }

                kvpParams = kvpParams.Union(encodedrequestBodyParams);

            }

            // Sort the parameters
            IEnumerable<string> sortedParams =
              from p in kvpParams
              orderby p.Key ascending, p.Value ascending
              select p.Key + "=" + p.Value;

            // Add the ampersand delimiter and then URL-encode
            string encodedParams = String.Join("&", sortedParams);
            encodedParams = Uri.EscapeDataString(encodedParams);
            return encodedParams;
        }
    }
}
