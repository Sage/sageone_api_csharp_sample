// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sage_One_Authorisation_Client.Contact_Helpers
{
    public class ContactTypes
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayed_as")]
        public string DisplayedAs { get; set; }

        [JsonProperty("$path")]
        public string Path { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }        
    }
}
