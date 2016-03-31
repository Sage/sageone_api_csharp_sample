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

            txtCompanyName.Text = "";
            txtConatctName.Text = "";
            txtContactTypeID.Text = "";

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
                ContactType type = contact.ContactType;

                string contactTypeID = type.Id.ToString();

                listText = contact.CompanyName + "," + contact.Name + "," + contact.Email + "," + contact.Telephone + "," + contactTypeID;

                item = new ListItem(listText, contact.Id.ToString());

                this.ListBoxContacts.Items.Add(item);
            }
        }

        protected void btnCreateContact_Click(object sender, EventArgs e)
        {
            string contactName = txtConatctName.Text;
            string companyName = txtCompanyName.Text;
            int contactTypeID = Int32.Parse(txtContactTypeID.Text);

            string token = (string)Session["token"];

            ContactManager contactManager = new ContactManager();

            string result = contactManager.CreateContact(contactName, companyName, "", "", contactTypeID, token);
                       
            ListContacts();

        }

        protected void ListBoxContacts_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedContact = ListBoxContacts.SelectedItem.Value.ToString();
            string text = ListBoxContacts.SelectedItem.Text.ToString();
            string[] contactValues = text.Split(',');
            txtConatctName.Text = contactValues[1];
            txtCompanyName.Text = contactValues[0];
            txtContactTypeID.Text = contactValues[4];
            
        }

        protected void btnUpdateContact_Click(object sender, EventArgs e)
        {
            string token = (string)Session["token"];
            ContactManager contactManager = new ContactManager();
            string result = contactManager.UpdateContact(ListBoxContacts.SelectedItem.Value.ToString(), 
                txtConatctName.Text, txtCompanyName.Text, "", "", 
                Int32.Parse(txtContactTypeID.Text), token);

            ListContacts();
        }

        protected void btnDeleteContact_Click(object sender, EventArgs e)
        {
            string token = (string)Session["token"];
            ContactManager contactManager = new ContactManager();
            string result = contactManager.DeleteContact(ListBoxContacts.SelectedItem.Value.ToString(), token);

            ListContacts();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtCompanyName.Text = "";
            txtConatctName.Text = "";
            txtContactTypeID.Text = "";
        }
    }
}