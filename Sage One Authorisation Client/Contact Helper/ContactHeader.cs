using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sage_One_Authorisation_Client.Contact_Helpers
{
    public partial class ContactGetHeader
    {
        [JsonProperty("$total")]
        public int Total { get; set; }

        [JsonProperty("$page")]
        public int Page { get; set; }

        [JsonProperty("$next")]
        public string Next { get; set; }

        [JsonProperty("$back")]
        public string Back { get; set; }

        [JsonProperty("$itemsPerPage")]
        public int ItemsPerPage { get; set; }

        [JsonProperty("$items")]
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
        public string Id { get; set; }

        [JsonProperty("contact_type_id")]
        public string ContactTypeID { get; set; }
    }

    public class ContactToGet : Contact
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayed_as")]
        public string DisplayedAs { get; set; }

        [JsonProperty("$path")]
        public string Path { get; set; }
    }

    public abstract class Contact
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("tax_number")]
        public string TaxNumber { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }

        [JsonProperty("main_address")]
        public MainAddress Address { get; set; }

        [JsonProperty("contact_types")]
        public List<ContactTypes> ContactTypes { get; set; }

    }    
}
