using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Sage_One_Authorisation_Client;

namespace Sage_One_API_Sample_Website
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        /// <summary>
        /// Authroise the client application to make calls against the API
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void button_authorise_onclick(object sender, EventArgs e)
        {
            SageOneOAuth oAuth = new SageOneOAuth();

            if (oAuth.Token == "")
            {
                Response.Redirect(oAuth.AuthorizationURL);
            }
            else
            {
                Session["token"] = oAuth.Token;
                Response.Redirect("Contacts.aspx");
            }
        }
    }
}