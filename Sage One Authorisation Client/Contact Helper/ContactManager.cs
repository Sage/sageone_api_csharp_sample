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

        private Uri contactUri = new Uri("https://api.sageone.com/accounts/v1/contacts/");
        
        public ContactGetHeader GetContacts(string token)
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();

            string JSON = webRequest.GetData(contactUri, token, oauth.SigningSecret);

            ContactGetHeader contact = JsonConvert.DeserializeObject<ContactGetHeader>(JSON);

            return contact;
        }

        public ContactGetHeader GetContacts(string token, string emailAddress)
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();

            UriBuilder uriWithParameters = new UriBuilder(contactUri.AbsoluteUri);

            uriWithParameters.Query = "email=" + emailAddress;

            string JSON = webRequest.GetData(uriWithParameters.Uri, token, oauth.SigningSecret);

            ContactGetHeader contact = JsonConvert.DeserializeObject<ContactGetHeader>(JSON);

            return contact;
        }

        public ContactGetHeader GetContactItemsPerPage(string token, string page)
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();

            UriBuilder uriWithParameters = new UriBuilder(contactUri.AbsoluteUri);

            uriWithParameters.Query = "$itemsPerPage=" + page;

            string JSON = webRequest.GetData(uriWithParameters.Uri, token, oauth.SigningSecret);

            ContactGetHeader contact = JsonConvert.DeserializeObject<ContactGetHeader>(JSON);

            return contact;
        }

        public ContactToGet GetContact(string id, string token)
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();

            Uri specificContactUri = new Uri(contactUri.AbsoluteUri + "/" + id);

            string JSON = webRequest.GetData(specificContactUri, token, oauth.SigningSecret);

            ContactToGet contact = JsonConvert.DeserializeObject<ContactToGet>(JSON);

            return contact;
        }

        public string CreateContact(string name, string companyName, string email, string telephone, int contactTypeID, string token)
        {

            SageOneWebRequest webRequest = new SageOneWebRequest();

            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>> {
              new KeyValuePair<string,string>("contact[name]", name),
              new KeyValuePair<string,string>("contact[email]",email),
              new KeyValuePair<string,string>("contact[contact_type_id]",contactTypeID.ToString()),
              new KeyValuePair<string,string>("contact[telephone]",telephone),
              new KeyValuePair<string,string>("contact[company_name]",companyName) };

            string _return = webRequest.PostData(contactUri, postData, token, oauth.SigningSecret);


            return _return;
        }

        public string UpdateContact(string id, string name, string companyName, string email, string telephone, int contactTypeID, string token)
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();

            Uri specificContactUri = new Uri(contactUri.AbsoluteUri + "/" + id);

            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>> {
              new KeyValuePair<string,string>("contact[name]", name),
              new KeyValuePair<string,string>("contact[email]",email),
              new KeyValuePair<string,string>("contact[contact_type_id]",contactTypeID.ToString()),
              new KeyValuePair<string,string>("contact[telephone]",telephone),
              new KeyValuePair<string,string>("contact[company_name]",companyName) };

            string _return = webRequest.PutData(specificContactUri, postData, token, oauth.SigningSecret);

            return _return;
        }

        public string DeleteContact(string id, string token)
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();

            Uri specificContactUri = new Uri(contactUri.AbsoluteUri + "/" + id);

            return webRequest.DeleteData(specificContactUri, token, oauth.SigningSecret);
        }

    }
}