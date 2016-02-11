using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Sage_One_Authorisation_Client;
using Sage_One_Authorisation_Client.Contact_Helpers;

namespace Sage_One_API_Sample_Website
{
    public partial class Contacts : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack == false)
            {
                ListContacts();
            }
        }

        private void ListContacts()
        {
            string token = (string)Session["token"];

            ContactManager contactManager = new ContactManager();

            ContactGetHeader contactHeader = contactManager.GetContacts(token);

            DisplayContacts(contactHeader);
        }

        private void DisplayContacts(ContactGetHeader contactHeader)
        {
            string listText;
            ListItem item;
            ContactToGet contact;

            ListBoxContacts.Items.Clear();

            for (int i = 0; i < contactHeader.Contacts.Count; i++)
            {
                contact = contactHeader.Contacts[i];

                listText = contact.CompanyName + "(" + contact.Name + ") " + contact.Email + " " + contact.Telephone;

                item = new ListItem(listText, contact.Id.ToString());

                this.ListBoxContacts.Items.Add(item);
            }
        }
    }
}