using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.Entity.Spatial;

namespace ReciclaFacil.Models
{
    /// <summary>
    ///   .Net utility for google geocoding API
    /// </summary>
    public class NetGoogleGeocoding
    {
        public GeocodeJsonResponse GoogleGeoCodeResponse;
        const string GoogleGeoCodeJsonServiceUrl = "http://maps.googleapis.com/maps/api/geocode/json?address={0}&sensor=true&components=country:BR";
        public NetGoogleGeocoding()
        {
            GoogleGeoCodeResponse = new GeocodeJsonResponse()
            {
                GeoCodes = new List<Geocode>()
            };
        }

        public DbGeometry ConvertLatLonToDbGeometry(string longitude, string latitude)
        {

            latitude = latitude.Replace(",", ".");
            longitude = longitude.Replace(",", ".");
            var point = string.Format("POINT({1} {0})", latitude, longitude);
            return DbGeometry.FromText(point);
        }

        /// <summary>
        ///   Performs Geocode and returns Object
        /// </summary>
        /// <param name="address"> 
        public GeocodeJsonResponse GoogleGeocode(string address)
        {
            using (var cli = new WebClient())
            {
                var addressToValidate = string.Format(GoogleGeoCodeJsonServiceUrl, HttpUtility.UrlEncode(address));
                var response = cli.DownloadString(new Uri(addressToValidate));
                return HydrateJson(response);
            }
        }
        /// <summary>
        /// generic method to read json values from json string
        /// </summary>
        /// <param name="token"> 
        /// <param name="field"> 
        /// <returns> </returns>
        static string GetJsonNodeValue(JToken token, string field)
        {
            return token["address_components"].Children().Any(x => x["types"].Values<string>().Contains(field))
                 ? token["address_components"].Children().First(x => x["types"].Values<string>().Contains(field))["short_name"].Value<string>()
                 : string.Empty;
        }
        GeocodeJsonResponse HydrateJson(string jsonResponse)
        {
            var results = (JObject)JsonConvert.DeserializeObject(jsonResponse);
            foreach (
            var googleGeoCode in
               results["results"].Children().Select(
                  token =>
            new Geocode
            {
                HouseNumber = GetJsonNodeValue(token, "street_number"),
                StreetAddress = GetJsonNodeValue(token, "route"),
                City = GetJsonNodeValue(token, "locality"),
                County = GetJsonNodeValue(token, "administrative_area_level_2"),
                State = GetJsonNodeValue(token, "administrative_area_level_1"),
                Zip = GetJsonNodeValue(token, "postal_code"),
                Country = GetJsonNodeValue(token, "country"),
                FullAddress = token["formatted_address"].Value<string>(),
                Types = token["types"].Values<string>().ToList(),
                LocationType = token["geometry"]["location_type"].Value<string>(),
                Latitude = token["geometry"]["location"]["lat"].Value<string>(),
                Longitude = token["geometry"]["location"]["lng"].Value<string>(),
                Status = string.Format("{0}", token["geometry"]["location_type"].Value<string>())
            }))
            {
                GoogleGeoCodeResponse.GeoCodes.Add(googleGeoCode);
            }
            return GoogleGeoCodeResponse;
        }
    }

    public class GeocodeJsonResponse
    {
        public List<Geocode> GeoCodes { get; set; }
    }
    public class Geocode
    {
        public string HouseNumber { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string FullAddress { get; set; }
        public List<string> Types { get; set; }
        public string LocationType { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Status { get; set; }
    }

}
