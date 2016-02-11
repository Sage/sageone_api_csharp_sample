// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sage_One_Authorisation_Client.Contact_Helpers
{
    public class MainAddress
    {
        [JsonProperty("street_one")]
        public string StreetOne { get; set; }

        [JsonProperty("street_two")]
        public string StreetTwo { get; set; }

        [JsonProperty("town")]
        public string Town { get; set; }

        [JsonProperty("county")]
        public string County { get; set; }

        [JsonProperty("postcode")]
        public string Postcode { get; set; }

        [JsonProperty("country_id")]
        public int Country { get; set; }

        [JsonProperty("$key")]
        public int Key { get; set; }
    }
}
