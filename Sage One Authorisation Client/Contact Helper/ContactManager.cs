using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Sage_One_Authorisation_Client.Contact_Helpers
{
    public class ContactManager
    {
        private SageOneOAuth oauth = new SageOneOAuth();
        private Uri contactUri = new Uri("https://api.columbus.sage.com/global/sageone/accounts/v3/contacts");

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

        public ContactGetHeader GetContactAllAttributes(string token)
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();

            UriBuilder uriWithParameters = new UriBuilder(contactUri.AbsoluteUri);

            uriWithParameters.Query = "attributes=all";

            string JSON = webRequest.GetData(uriWithParameters.Uri, token, oauth.SigningSecret);

            ContactGetHeader contact = JsonConvert.DeserializeObject<ContactGetHeader>(JSON);

            return contact;
        }

        public ContactGetHeader GetContactItemsPerPage(string token, string page)
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();

            UriBuilder uriWithParameters = new UriBuilder(contactUri.AbsoluteUri);

            uriWithParameters.Query = "items_per_page=" + page;

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

        public string CreateContact(string name, string companyName, string email, string telephone, string contactTypeID, string token)
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();

            StringBuilder postDataBuilder = new StringBuilder();
            postDataBuilder.Append("contact[name]=" + name + "&");
            postDataBuilder.Append("contact[email]=" + email + "&");
            postDataBuilder.Append("contact[contact_type_ids][]=" + contactTypeID.ToString() + "&");
            postDataBuilder.Append("contact[telephone]=" + telephone + "&");
            postDataBuilder.Append("contact[company_name]=" + companyName);

            string _return = webRequest.PostData(contactUri, postDataBuilder.ToString(), token, oauth.SigningSecret);
            
            return _return;
        }

        public string UpdateContact(string id, string name, string companyName, string email, string telephone, string contactTypeID, string token)
        {
            SageOneWebRequest webRequest = new SageOneWebRequest();
            Uri specificContactUri = new Uri(contactUri.AbsoluteUri + "/" + id);

            StringBuilder putDataBuilder = new StringBuilder();
            putDataBuilder.Append("contact[name]=" + name + "&");
            putDataBuilder.Append("contact[contact_type_ids][]=" + contactTypeID.ToString());

            string _return = webRequest.PutData(specificContactUri, putDataBuilder.ToString(), token, oauth.SigningSecret);

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