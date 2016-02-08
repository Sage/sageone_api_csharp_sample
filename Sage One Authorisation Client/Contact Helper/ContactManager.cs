using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sage_One_Authorisation_Client.Contact_Helpers
{
    public class ContactManager
    {
        private SageOneOAuth oauth = new SageOneOAuth();

        private string baseURL = "https://api.sageone.com/accounts/v1/contacts/";
        
        public ContactGetHeader GetContacts(string token)
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();

            string JSON = webRequest.GetData(baseURL, "", token, oauth.SigningSecret);

            ContactGetHeader contact = JsonConvert.DeserializeObject<ContactGetHeader>(JSON);

            return contact;
        }
        
        public ContactGetHeader GetContacts(string token, string emailAddress)
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();

            string JSON = webRequest.GetData(baseURL, webRequest.PercentEncodeRfc3986("email") + "=" + webRequest.PercentEncodeRfc3986(emailAddress), token, oauth.SigningSecret);

            ContactGetHeader contact = JsonConvert.DeserializeObject<ContactGetHeader>(JSON);

            return contact;
        }

        public ContactGetHeader GetContactItemsPerPage(string token, string page)
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();

            string JSON = webRequest.GetData(baseURL, webRequest.PercentEncodeRfc3986("$itemsPerPage") + "=" + webRequest.PercentEncodeRfc3986(page), token, oauth.SigningSecret);

            ContactGetHeader contact = JsonConvert.DeserializeObject<ContactGetHeader>(JSON);

            return contact;
        }
        
        public ContactToGet GetContact(string id, string token)
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();

            string url = baseURL + "/" + id;

            string JSON = webRequest.GetData(url, "", token, oauth.SigningSecret);

            ContactToGet contact = JsonConvert.DeserializeObject<ContactToGet>( JSON );

            return contact;
        }

        public string CreateContact( string name, string companyName, string email, string telephone, int contactTypeID, string token)
        {

            SageOneWebRequest webRequest = new SageOneWebRequest();

            string formattedName = webRequest.PercentEncodeRfc3986("contact[name]") + "=" + webRequest.PercentEncodeRfc3986(name);
            string formattedCompanyName = webRequest.PercentEncodeRfc3986("contact[company_name]") + "=" + webRequest.PercentEncodeRfc3986(companyName);
            string formattedEmail = webRequest.PercentEncodeRfc3986("contact[email]") + "=" + webRequest.PercentEncodeRfc3986(email);
            string formattedTelephone = webRequest.PercentEncodeRfc3986("contact[telephone]") + "=" + webRequest.PercentEncodeRfc3986(telephone);
            string formattedContactTypeID = webRequest.PercentEncodeRfc3986("contact[contact_type_id]") + "=" + webRequest.PercentEncodeRfc3986(Convert.ToString("1"));
            string postData = formattedCompanyName + "&" + formattedContactTypeID + "&" + formattedEmail + "&" + formattedName  + "&" + formattedTelephone;

            string nonce = Guid.NewGuid().ToString("N");        

            string _return = webRequest.PostData(baseURL, postData, token, oauth.SigningSecret);            
         
            
            return _return;
        }

        public string UpdateContact(string id, string name, string companyName, string email, string telephone, int contactTypeID, string token)
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();

            string url = baseURL + id;

            string formattedName = webRequest.PercentEncodeRfc3986("contact[name]") + "=" + webRequest.PercentEncodeRfc3986(name);
            string formattedCompanyName = webRequest.PercentEncodeRfc3986("contact[company_name]") + "=" + webRequest.PercentEncodeRfc3986(companyName);
            string formattedEmail = webRequest.PercentEncodeRfc3986("contact[email]") + "=" + webRequest.PercentEncodeRfc3986(email);
            string formattedTelephone = webRequest.PercentEncodeRfc3986("contact[telephone]") + "=" + webRequest.PercentEncodeRfc3986(telephone);
            string formattedContactTypeID = webRequest.PercentEncodeRfc3986("contact[contact_type_id]") + "=" + webRequest.PercentEncodeRfc3986(Convert.ToString(contactTypeID));
            string postData = formattedCompanyName + "&" + formattedContactTypeID + "&" + formattedEmail + "&" + formattedName + "&" + formattedTelephone;

            string _return = webRequest.PutData(url, postData, token, oauth.SigningSecret);

            return _return;
        }
        
        public string DeleteContact(string id, string token )
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();

            string url = baseURL + id;

            return webRequest.DeleteData(url, "",token, oauth.SigningSecret);
        }

    }
}