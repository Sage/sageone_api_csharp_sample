using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sage_One_Authorisation_Client;
using Sage_One_Authorisation_Client.RequestHelper;

namespace Sage_One_API_Sample_Website
{
    public partial class Requests : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnMakeRequest_Click(object sender, EventArgs e)
        {
            string method = methodDropDownList.SelectedValue.ToString();
            string country = Session["country"].ToString();
            string endpoint = txtEndpoint.Text.ToLower();
            string baseurl = String.Format("https://api.columbus.sage.com/{0}/sageone/accounts/v3/{1}", country.ToLower(), endpoint );
            Uri endpointUri = new Uri(baseurl);

            string token = Session["token"].ToString();
            string siteID = Session["site_id"].ToString();

            RequestHelper request = new RequestHelper();

            string result = "";

            switch (method)
            {
                case "GET":
                    result = request.GetRequest(endpointUri, token, siteID);
                    break;
                case "POST":
                    result = request.PostRequest(endpointUri, txtAreaRequestBody.Text, token, siteID);
                    break;
                case "PUT":
                    result = request.PutRequest(endpointUri, txtAreaRequestBody.Text, token, siteID);
                    break;
                case "DELETE":
                    result = request.DeleteRequest(endpointUri, token, siteID);
                    break;
            }

            var obj = JsonConvert.DeserializeObject(result);
            var formatted = JsonConvert.SerializeObject(obj, Formatting.Indented);
            txtAreaResult.Text = formatted;
        }
    }
}