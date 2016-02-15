// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sage_One_Authorisation_Client.Contact_Helpers
{
    public class ContactType
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("$key")]
        public int Key { get; set; }
    }
}
