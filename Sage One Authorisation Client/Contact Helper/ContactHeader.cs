using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sage_One_Authorisation_Client.Contact_Helpers
{
    public partial class ContactGetHeader
    {
        [JsonProperty("$totalResults")]
        public int TotalResults { get; set; }

        [JsonProperty("$startIndex")]
        public int StartIndex { get; set; }

        [JsonProperty("$itemsPerPage")]
        public int ItemsPerPage { get; set; }

        [JsonProperty("$resources")]
        public List<ContactToGet> Contacts { get; set; }
    }

    public class ContactPostHeader
    {
        [JsonProperty("contact")]
        public ContactToPost Contact { get; set; }
    }

    public class ContactToPost : Contact
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("contact_type_id")]
        public int ContactTypeID { get; set; }
    }

    public class ContactToGet : Contact
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("contact_type")]
        public ContactType ContactType { get; set; }
    }

    public abstract class Contact
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("company_name")]
        public string CompanyName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("telephone")]
        public string Telephone { get; set; }

        [JsonProperty("main_address")]
        public MainAddress Address { get; set; }
    }    
}
