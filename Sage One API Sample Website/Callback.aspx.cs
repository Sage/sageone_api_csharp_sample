using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Sage_One_Authorisation_Client;

namespace Sage_One_API_Sample_Website
{
    public partial class Callback : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            InitiateCallBack();
        }

        private void InitiateCallBack()
        {
            string code = Request.QueryString["code"];
            string error = Request.QueryString["error"];
            string country = Request.QueryString["country"]; 

            if (country != null)
            {
                Session["country"] = country;
            }

            if ((code == null) && (error == null))
            {
                //There is no code, an error has occured, signal to the user
                this.LabelStatus.Text = "An error has occured signing in to Sage One.  <p>Please press back on your browser to start again.";
            }

            if (error != null)
            {
                //The user has denied access to Sage One
                this.LabelStatus.Text = "You denied access to your Sage One data.  <p>If this was an error, please press back on your browser to start the process again.";
            }

            if (code != null)
            {
                //Stage One of the authentication process has been completed - get the Access Token from Sage One
                GetAccessToken(code, country);
            }
        }

        private void GetAccessToken(string code, string country)
        {
            SageOneOAuth oAuth = new SageOneOAuth();

            oAuth.GetAccessToken(code, country);
            
            this.LabelStatus.Text = "<p><a href='Requests.aspx'>Make API requests</a>";

            Session["token"] = oAuth.Token;
            Session["country"] = oAuth.Country;
            Session["site_id"] = oAuth.SiteID;

        }

    }
}